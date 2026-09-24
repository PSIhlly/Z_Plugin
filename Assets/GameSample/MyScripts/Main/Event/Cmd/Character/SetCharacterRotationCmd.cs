using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Z_Code.Form;
using Z_DataSystem;
using Z_Map;
using Z_Map.Form;
using Z_Math;
using Z_Text;
using Z_Time;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class SetCharacterRotationCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetCharacterRotationCmd());
        }
        public override string GetName() => "SetCharacterRotation";
        public override CmdBase GetNew() => new SetCharacterRotationCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.interpreter.data.heapTemp.Clear();
            foreach (var p in prm)
                asyncTask.interpreter.data.heapTemp.Add(p.DeepCopy());
            var heapTemp = asyncTask.interpreter.data.heapTemp;
            var productData = CharacterProductForm.DataByUid[GlobalEventHelper.GetId(heapTemp[0].str, GlobalEventHelper.CHARACTER)];
            var data = PlayManager.instance.sceneCtrl.GetCharacterUnit(productData.uid);
            if (!(data?.unit is CharacterUnit unit))
                return true;

            float targetYaw = Mathf.Repeat(heapTemp[1].num, 360f);

            void Set(float y)
            {
                Vector3 euler = data.euler.NewSetY(Mathf.Repeat(y, 360f));
                unit.forceEuler = euler;
                data.euler = euler;
                if (unit.ins != null)
                    unit.ins.transform.eulerAngles = euler;
            }

            if (heapTemp[2].num <= 0)
            {
                Set(targetYaw);
                return true;
            }

            float duration = heapTemp[2].num;
            float startYaw = data.euler.y;
            float elapsed = 0f;
            GameManager.instance.evtCtrl.StartTask(asyncTask, () =>
            {
                if (!ReferenceEquals(data.unit, unit))
                {
                    asyncTask.Complete();
                    return true;
                }

                elapsed = Mathf.Min(duration, elapsed + Time.deltaTime);
                Set(elapsed >= duration
                    ? targetYaw
                    : Mathf.LerpAngle(startYaw, targetYaw, elapsed / duration));
                if (elapsed >= duration)
                {
                    asyncTask.Complete();
                    return true;
                }
                return false;
            });

            return false;
        }
    }
}
