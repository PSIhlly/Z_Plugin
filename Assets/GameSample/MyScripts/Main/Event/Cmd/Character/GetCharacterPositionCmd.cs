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
    public class GetCharacterPositionCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetCharacterPositionCmd());
        }
        public override string GetName() => "GetCharacterPosition";
        public override CmdBase GetNew() => new GetCharacterPositionCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = PlayManager.instance.sceneCtrl.GetCharacterUnit(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.CHARACTER));
            if(data!=null)
            {
                var box=CodeHelper.CreateBox();
                var vec = MapManager.instance.utilCtrl.RealPos2MapPos(data.pos);
                box.dic["x"] = CodeHelper.CreateBoxByNum(vec.x);
                box.dic["height"] = CodeHelper.CreateBoxByNum(vec.y);
                box.dic["y"] = CodeHelper.CreateBoxByNum(vec.z);
                asyncTask.res = new BoxDataForm.Data[] { box };
            }else
            {
                Debug.LogError($"GetCharacterPositionCmd error: GetCharacterUnit can't find character with id {prm[0].str}");
            }
            return true;
        }
    }
}
