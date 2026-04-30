using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;
using Z_Ui.Dialog;
using Z_Ui.Notify;

namespace Z_Code
{
    public class ShowDialogWithChoiceCmd : CmdBase
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new ShowDialogWithChoiceCmd());
        }
        public override string GetName() => "ShowDialogWithChoice";
        public override CmdBase GetNew() => new ShowDialogWithChoiceCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask)
        {
            asyncTask.interpreter.data.heapTemp.Clear();
            foreach (var p in prm)
                asyncTask.interpreter.data.heapTemp.Add(p.DeepCopy());
            var heapTemp = asyncTask.interpreter.data.heapTemp;


            EntryItem items = new EntryItem();
            List<BoxDataForm.Data> values = new List<BoxDataForm.Data>(prm[4].dic.Values);
            for (int i = 0; i < values.Count; i++)
            {
                items.Add(values[i].str, null, i);
            }
            bool over = true;
            if (items.subs.Count > 0)
            {
                NotifyManager.instance.AddQuickChoose((item) =>
                {
                    asyncTask.res = new BoxDataForm.Data[] { values[item.id] };
                    DialogManager.instance.Close();
                    asyncTask.Complete();
                    return true;
                }, items);
                over = false;
            }
            DialogManager.instance.Begin(heapTemp[2].str, heapTemp[3].str, heapTemp[0].str, "", heapTemp[1].str, "", () => { if (over) asyncTask.Complete(); }, over);
            return false;
        }
    }
}
