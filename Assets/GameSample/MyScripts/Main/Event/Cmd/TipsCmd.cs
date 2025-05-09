using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui;
using Z_Ui.Notify;

public class TipsCmd : CmdBase
{

    public override CmdRes Execute(CmdForm.Data self,VarForm.Data[] prs, float progress)
    {
        NotifyManager.instance.AddTip(prs[0].s);
        return new CmdRes()
        { 
        };
    }
}
