using Form;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code.Form;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Ui.Dialog;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

namespace Z_Code
{
    public class GetTriggerCharacterIDCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new GetTriggerCharacterIDCmd());
        }
        public override string GetName() => "GetTriggerCharacterID";
        public override CmdBase GetNew() => new GetTriggerCharacterIDCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            int unitUid = (int)((EventInterpretDataForm.Data)asyncTask.interpreter.data).args[1].num;
            var data = CharacterUnitForm.DataByUid[unitUid];
            if(data!=null)
            {
                asyncTask.res = new BoxDataForm.Data[] { CodeHelper.CreateBoxByNum(data.unit.productInfo.Item1) };
            }
            return true;
        }
    }
}
