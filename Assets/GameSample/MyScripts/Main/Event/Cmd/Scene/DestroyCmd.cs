using System;
using System.Collections.Generic;
using System.Globalization;
using Form;
using UnityEngine;
using Z_Code.Form;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class DestroyCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new DestroyCmd());
            RegisterAlias("DestroyCharacter", new DestroyCmd());
            RegisterAlias("DestroyObject", new DestroyCmd());
        }

        public override string GetName() => "Destroy";
        public override CmdBase GetNew() => new DestroyCmd();

        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            if (prm.Length == 0 || string.IsNullOrEmpty(prm[0].str))
                return true;

            // Capture all targets before any removal callback can change the
            // registries, Product references, or another target's identity.
            var targets = Snapshot(prm[0].str);
            var controller = GameManager.instance.evtCtrl;
            targets.RemoveAll(target => !SceneUnitCommand.IsLive(target.unit, target.data)
                || !controller.BeginUnitDestroy(target.data));
            if (targets.Count == 0) return true;
            var operation = new PendingDestroy(controller, targets);
            try
            {
                bool pending = false;
                foreach (var target in targets)
                {
                    var trigger = target.unit.GetEvt("onDestroyEvent");
                    if (trigger?.evt == null || trigger.evt.Count == 0) continue;
                    target.executions = new List<EventInterpretDataForm.Data>();
                    controller.TriggerEventExecute(trigger, target.data.uid, new Dictionary<string, BoxDataForm.Data>
                    {
                        { "param1", CodeHelper.CreateBoxByStr(GlobalEventHelper.GetSceneUnitReference(target.data.uid)) }
                    }, copyDefaultHeap: true, executions: target.executions, blockingParent: GameEventController.RootEvent(asyncTask));
                    pending |= target.executions.Count > 0;
                }
                if (!pending)
                {
                    operation.Commit();
                    return true;
                }
                controller.StartTask(asyncTask, () => operation.Update(asyncTask), operation.Cancel);
                return false;
            }
            catch
            {
                operation.Cancel();
                throw;
            }
        }

        private static void RemoveTargets(List<Target> targets)
        {
            var removedCharacterProducts = new List<CharacterProductForm.Data>();
            foreach (var target in targets)
            {
                if (!SceneUnitCommand.IsLive(target.unit, target.data))
                    continue;

                if (target.unit is CharacterUnit character)
                {
                    if (PlayManager.instance.sceneCtrl.RemoveCharacter(character.data)
                        && target.characterProduct != null && !target.preserveCharacterProduct)
                        removedCharacterProducts.Add(target.characterProduct);
                }
                else if (target.unit is ObjectUnit obj)
                    MapManager.instance.RemoveObject(obj.data);
                else if (target.unit is ItemUnit item)
                    MapManager.instance.RemoveItem(item.data);
            }

            // Keep a shared Product available until all selected scene Units
            // have removed their Play lookup entries. Never delete a replacement
            // Product that a removal callback registered with a recycled UID.
            foreach (var product in removedCharacterProducts)
                if (CharacterProductForm.DataByUid.TryGetValue(product.uid, out var registered)
                    && ReferenceEquals(registered, product) && !product.unique)
                    CharacterProductForm.RemoveData(product.uid);
        }

        private sealed class Target
        {
            public MapUnit unit;
            public UnitForm.Data data;
            public CharacterProductForm.Data characterProduct;
            public bool preserveCharacterProduct;
            public List<EventInterpretDataForm.Data> executions;
        }

        private sealed class PendingDestroy
        {
            private readonly GameEventController controller;
            private readonly List<Target> targets;
            private readonly SceneForm.Data scene;
            private bool finished, committing;

            public PendingDestroy(GameEventController controller, List<Target> targets)
            {
                this.controller = controller;
                this.targets = targets;
                scene = GameManager.instance.curScene;
            }

            public bool Update(InterpretAsyncTask task)
            {
                if (finished) return true;
                if (!ReferenceEquals(GameManager.instance.curScene, scene)
                    || GameManager.instance.curProgress.eventState == EventState.Leave)
                {
                    Cancel();
                    task.Cancel();
                    return true;
                }
                foreach (var target in targets)
                    if (target.executions != null)
                        foreach (var execution in target.executions)
                            if (GameEventController.IsPendingEvent(execution)) return false;
                Commit();
                task.Complete();
                return true;
            }

            public void Commit()
            {
                if (finished) return;
                committing = true;
                try { RemoveTargets(targets); }
                finally { committing = false; Release(); }
            }

            public void Cancel()
            {
                // Removal of the caller's own Unit can cancel its task during
                // Commit. Keep all reservations until that removal batch ends.
                if (finished || committing) return;
                finished = true;
                foreach (var target in targets)
                    if (target.executions != null) controller.CancelTriggerEvents(target.executions);
                Release();
            }

            private void Release()
            {
                finished = true;
                foreach (var target in targets) controller.EndUnitDestroy(target.data);
            }
        }

        private static List<Target> Snapshot(string reference)
        {
            List<MapUnit> units;
            if (TryGetItemProductUid(reference, out int itemUid))
            {
                units = new List<MapUnit>();
                // An Item handle selects its current scene representations,
                // not its backpack stack, template or stored Product.
                if (ItemProductForm.DataByUid.ContainsKey(itemUid))
                    foreach (var data in ItemUnitForm.DataByUid.Values)
                        if (data?.unit is ItemUnit item && SceneUnitCommand.IsLive(item, data)
                            && item.productInfo.Item1 == itemUid)
                            units.Add(item);
            }
            else
                units = SceneUnitCommand.ResolveAll(reference);

            var result = new List<Target>();
            foreach (var unit in units)
            {
                if (!(unit is CharacterUnit || unit is ObjectUnit || unit is ItemUnit))
                    continue;
                var target = new Target { unit = unit, data = unit.data };
                if (unit is CharacterUnit character)
                {
                    CharacterProductForm.DataByUid.TryGetValue(character.productInfo.Item1, out target.characterProduct);
                    target.preserveCharacterProduct = target.characterProduct?.unique == true;
                }
                result.Add(target);
            }
            return result;
        }

        private static bool TryGetItemProductUid(string reference, out int uid)
        {
            uid = 0;
            string mark = GlobalEventHelper.ITEM;
            return reference.Length > mark.Length * 2
                && reference.StartsWith(mark, StringComparison.Ordinal)
                && reference.EndsWith(mark, StringComparison.Ordinal)
                && int.TryParse(reference.Substring(mark.Length, reference.Length - mark.Length * 2),
                    NumberStyles.None, CultureInfo.InvariantCulture, out uid) && uid > 0;
        }
    }
}
