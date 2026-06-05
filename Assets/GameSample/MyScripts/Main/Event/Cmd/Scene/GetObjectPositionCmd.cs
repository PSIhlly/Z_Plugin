using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GetObjectPositionCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetObjectPositionCmd());
        }
        public override string GetName() => "GetObjectPosition";
        public override CmdBase GetNew() => new GetObjectPositionCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            
            var data = UnitForm.DataByUid.GetDv(GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.SCENEOBJECT),null); 
            var box = CodeHelper.CreateBox();
            if (data!=null)
            {
                
                var vec = GameManager.MapPosToPlayerPos(MapManager.instance.utilCtrl.RealPos2MapPos(data.pos));
                box.dic["x"] = CodeHelper.CreateBoxByNum(vec.x);
                box.dic["height"] = CodeHelper.CreateBoxByNum(vec.y);
                box.dic["y"] = CodeHelper.CreateBoxByNum(vec.z);
            }else
            {
                box.dic["x"] = CodeHelper.CreateBoxByNum(0);
                box.dic["height"] = CodeHelper.CreateBoxByNum(0);
                box.dic["y"] = CodeHelper.CreateBoxByNum(0);
            }
            asyncTask.res = new BoxDataForm.Data[] { box };
            return true;
        }
    }
}
