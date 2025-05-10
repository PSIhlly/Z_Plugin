using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Notify;

public class NumCmd : CmdBase
{
    public override CmdRes Execute(CmdForm.Data self,VarForm.Data[] prs, float progress)
    {
        return new CmdRes()
        {
            v = new List<VarForm.Data>() { new VarForm.Data(-1,"",0, float.Parse(self.constV),null,null,0) }
        };
    }

}
