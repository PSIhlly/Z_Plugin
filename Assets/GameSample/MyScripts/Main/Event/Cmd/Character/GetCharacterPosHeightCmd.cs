using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GetCharacterPosHeightCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetCharacterPosHeightCmd());
        }
        public override string GetName() => "GetCharacterPosHeight";
        public override CmdBase GetNew() => new GetCharacterPosHeightCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = PlayManager.instance.sceneCtrl.GetCharacterUnit(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER));
            if(data!=null)
            {
                asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByNum(MapManager.instance.utilCtrl.RealPos2MapPos(data.pos).y) };
            }
            return true;
        }
    }
}
