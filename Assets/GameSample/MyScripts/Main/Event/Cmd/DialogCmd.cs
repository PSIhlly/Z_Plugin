using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_Texture;
using Z_Ui.Dialog;
using Z_Ui.Notify;

public class DialogCmd : CmdBase
{
    private float localProgress;
    public override CmdRes Execute(CmdForm.Data self,VarForm.Data[] prs,float progress)
    {
        if (prs[0].clips.Count==0)
        {
            new CmdRes();
        }
        if (localProgress==0)
        {
            localProgress = 0.1f;

            DialogManager.instance.Begin(prs[0].clips,
           
                () => {  localProgress = 1; });
        }
        return new CmdRes()
        {
            progress = localProgress
        };
    }

}
