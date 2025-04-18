using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui;
using Z_Ui.Notify;

public class TipsCmd : CmdBase
{

    public override CmdRes Execute(object[] prs)
    {
        NotifyManager.instance.AddTip((string)prs[0]);
        return new CmdRes()
        { 
        };
    }
}
