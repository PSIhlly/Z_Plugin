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
        if(localProgress==0)
        {
            localProgress = 0.1f;
            DialogManager.instance.Begin(new List<string>() { prs[0].s }, new List<string>() { prs[0].s }, new List<Sprite>() { TextureHelper.transparentSprite }, new List<Sprite>() { TextureHelper.transparentSprite },
           
                () => {  localProgress = 1; });
        }
        return new CmdRes()
        {
            progress = localProgress
        };
    }

}
