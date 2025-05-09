using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Notify;

public class EmptyCmd : CmdBase
{
    public override CmdRes Execute(CmdForm.Data self, VarForm.Data[] prs, float progress)
    {
        return new CmdRes()
        {
            v = new List<VarForm.Data>() { VarForm.defaultData }
        };
    }

}
