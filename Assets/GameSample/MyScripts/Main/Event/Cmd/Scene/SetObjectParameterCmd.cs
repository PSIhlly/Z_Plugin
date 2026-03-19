using Form;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public class SetObjectParameterCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new SetObjectParameterCmd());
        }
        public override string GetName() => "SetObjectParameter";
        public override CmdBase GetNew() => new SetObjectParameterCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            var data = UnitForm.DataByUid[GlobalEventHelper.GetId(prm[0].str, GlobalEventHelper.SCENEOBJECT)];
            if (data != null && ((MapUnit)(data.unit)).paramInfo.ContainsKey(prm[1].str))
            {
                ((MapUnit)(data.unit)).paramInfo[prm[1].str].v = prm[2].num;
                ((MapUnit)(data.unit)).paramInfo = ((MapUnit)(data.unit)).paramInfo;
            }
          
            return true;
        }
    }
}
