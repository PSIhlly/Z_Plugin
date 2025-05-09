using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Notify;

public class IfCmd : CmdBase
{
    public override CmdRes Execute(CmdForm.Data self,VarForm.Data[] prs, float progress)
    {

        return new CmdRes()
        {
            ignoreUntilCmd= prs[0].v==1?"":"then",
            ignoreTimesCmd="if"
        };
    }

}
