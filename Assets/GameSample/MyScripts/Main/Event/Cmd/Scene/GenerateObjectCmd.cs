using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GenerateObjectCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GenerateObjectCmd());
        }
        public override string GetName() => "GenerateObject";
        public override CmdBase GetNew() => new GenerateObjectCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.res = new[] { CodeHelper.CreateBoxByNum(0) };
            var data = GlobalEventHelper.ResolveSceneObjectTemplate(prm[0].str);
            var map = MapManager.instance;
            if (data != null && TryPosition(prm[1], out var position)
                && map.data != null && map.utilCtrl != null && map.updateCtrl != null
                && map.utilCtrl.GetPlacementTile(position) != null)
            {
                var key = data.uid.ToString();

                var newObjectData = map.AddObject(key, position, GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(ObjectProductForm.GetTemplateUid(data)), null);

                if (newObjectData != null)
                {
                    // Selecting an existing instance copies its Product details;
                    // the new placement must never share mutable state with it.
                    if (data.protoUid != 0)
                    {
                        var copy = data.Copy(false);
                        copy.ToProduct(ObjectProductForm.GetTemplateUid(data));
                        data = copy;
                    }
                    GameManager.instance.mapCtrl.RegisterObject(newObjectData, data);
                    MapManager.instance.updateCtrl.UpdateSingleOne(newObjectData.unit);
                    asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByStr(GlobalEventHelper.GetSceneObjectReference(newObjectData.unit)) };
                    return true;
                }
            }
            return true;
        }

        private static bool TryPosition(BoxDataForm.Data value, out Vector3 position)
        {
            position = default;
            if (value?.dic == null || !value.dic.TryGetValue("x", out var x)
                || !value.dic.TryGetValue("y", out var y) || !value.dic.TryGetValue("height", out var height)
                || !IsNumber(x) || !IsNumber(y) || !IsNumber(height)) return false;
            position = GameManager.PlayerPosToMapPos(new Vector3(x.num, height.num, y.num));
            return true;
        }

        private static bool IsNumber(BoxDataForm.Data value) => value != null && value.str == null
            && !float.IsNaN(value.num) && !float.IsInfinity(value.num);
    }
}
