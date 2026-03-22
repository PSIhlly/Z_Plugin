using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Z_Code.Form;
using Z_DataSystem;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Time;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;
using static UnityEditor.PlayerSettings;

namespace Z_Code
{
    public class MoveCharacterRelativeCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new MoveCharacterRelativeCmd());
        }
        public override string GetName() => "MoveCharacterRelative";
        public override CmdBase GetNew() => new MoveCharacterRelativeCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {

            var productData = CharacterProductForm.DataByUid[GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER)];
            var data = PlayManager.instance.sceneCtrl.GetCharacterUnit(productData.uid);
            Debug.Log("dir:" + prm[1].dic["height"].num);
            GameManager.instance.evtCtrl.StartTask(() =>
            {
                var relaPos = new Vector3(prm[1].dic["x"].num, prm[1].dic["height"].num, prm[1].dic["y"].num);
                var time = Mathf.Max(0.0001f, prm[2].num);
                var step = Mathf.Min(1, Time.deltaTime / time) * relaPos;


                var oldPos = data.pos;
                
                {
                    if (data.unit is CharacterUnit o)
                    {
                        o.Move(step);
                    }
                }
                var realStep = (data.pos - oldPos);
                prm[1].dic["x"].num -= realStep.x;
                prm[1].dic["z"].num -= realStep.z;
                prm[1].dic["height"].num -= realStep.y;
                prm[2].num -= Time.deltaTime;
                if (prm[2].num <= 0)
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
