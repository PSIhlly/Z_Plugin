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
using Z_Text;
using Z_Time;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class MoveCharacterAbsoluteCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new MoveCharacterAbsoluteCmd());
        }
        public override string GetName() => "MoveCharacterAbsolute";
        public override CmdBase GetNew() => new MoveCharacterAbsoluteCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var productData = CharacterProductForm.DataByUid[GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER)];
            var data = PlayManager.instance.sceneCtrl.GetCharacterUnit(productData.uid);
            GameManager.instance.evtCtrl.StartTask(() =>
            {
                var pos = MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(prm[1].dic["x"].num, prm[1].dic["height"].num, prm[1].dic["y"].num));
                var time = prm[2].num;
                var oldPos = data.pos;
                var step = Mathf.Min(1, Time.deltaTime / time) * (pos - oldPos) + oldPos;


                if (time <= 0)
                {
                    asyncTask.Complete();
                    return true;
                }
                else
                {
                    if (data.unit is CharacterUnit o)
                    {
                        o.Move(step);
                    }
                }
                prm[2].num -= Time.deltaTime;
                return false;
            });

            return false;
        }
    }
}
