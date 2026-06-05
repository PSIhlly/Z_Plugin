using System;
using System.Collections;
using System.Collections.Generic;
using Ui.DialogBg;
using UnityEngine;
using Z_Code.Form;
using Z_Ui;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class MakeChoiceCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new MakeChoiceCmd());
        }
        public override string GetName() => "MakeChoice";
        public override CmdBase GetNew() => new MakeChoiceCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.interpreter.data.heapTemp.Clear();

            EntryItem items = new EntryItem();
            List<BoxDataForm.Data> values = new List<BoxDataForm.Data>(prm[0].dic.Values);
            for(int i=0;i< values.Count;i++)
            {
                items.Add(values[i].str,null,i);
            }
            NotifyManager.instance.AddQuickChoose((item) =>
            {
                asyncTask.res = new BoxDataForm.Data[] { values[item.id] };
                asyncTask.Complete();
                return true;
            }, items);
            return false;
        }
    }
}
