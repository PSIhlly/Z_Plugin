using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_Map;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class SetSceneObjectParameterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetSceneObjectParameterCmd());
        }
        public override string GetName() => "SetSceneObjectParameter";
        public override CmdBase GetNew() => new SetSceneObjectParameterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = UnitForm.DataByUid[GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.SCENEOBJECT)];
          
            if (data != null)
            {
                var dic = ((MapUnit)(data.unit)).paramInfo;
                if(!dic.ContainsKey(prm[1].str))
                    dic[prm[1].str] = new MapObjectParamForm.Data(-1, prm[1].str, 0, "", "", "");
                dic[prm[1].str].SetValue(prm[2]);
                ((MapUnit)(data.unit)).paramInfo = dic;
            }
          
            return true;
        }
    }
}
