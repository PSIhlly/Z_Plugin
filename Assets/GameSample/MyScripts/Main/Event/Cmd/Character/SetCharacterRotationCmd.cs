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

            void Set(float y)
            {
                data.unit.forceEuler = data.euler.NewSetY(y);
                data.euler = data.euler.NewSetY(y);
                if (data.unit.ins != null)
                {
                    data.unit.ins.transform.eulerAngles = data.unit.ins.transform.eulerAngles.NewSetY(y);
                }
            }

            if (heapTemp[2].num <= 0)
            {
                Set(heapTemp[1].num);
                return true;
            }
            GameManager.instance.evtCtrl.StartTask(() =>
            {
                var euler = heapTemp[1].num;
                var time = Mathf.Max(heapTemp[2].num,0.0001f);
                
                
                if(data!=null)
                {
                    var step = Mathf.Min(1, Time.deltaTime / time) * (euler - data.euler.y);
                    Set(step);
                }
                heapTemp[2].num -= Time.deltaTime;
                if (heapTemp[2].num <= 0)
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
