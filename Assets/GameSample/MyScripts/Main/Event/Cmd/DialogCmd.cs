using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Notify;

public class DialogCmd : CmdBase
{
    public override CmdRes Execute(VarForm.Data[] prs)
    {
        NotifyManager.instance.AddTip(prs[0].s);
        return new CmdRes()
        {
        };
    }

}
