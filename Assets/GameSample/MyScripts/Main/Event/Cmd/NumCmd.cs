using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Z_Code
{
    public class NumCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new NumCmd());
        }
        public override void GetUnitChooseCode(Action<string> act)
        {
            NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("input value"), true, (res) =>
            {
                if(float.TryParse(res, out float val))
                {
                    act?.Invoke(val.ToString());
                }
                return true;
            });
        }
        public override string GetName() => "Text";
        public override CmdBase GetNew() => new NumCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            return true;
        }
    }
}
