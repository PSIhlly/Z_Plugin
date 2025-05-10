using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_ByteSerialize;
using Z_Ui.Form;
using Z_Ui.Notify;

public class DialogClipCmd : CmdBase
{
    public override CmdRes Execute(CmdForm.Data self, VarForm.Data[] prs, float progress)
    {

        if (string.IsNullOrEmpty(self.constV))
        {
            return new CmdRes()
            {
                v = new List<VarForm.Data>() { new VarForm.Data(-1, "", 7, 0, "", new List<ClipForm.Data>(), 0) }
            };
        }
        var jo = JObject.Parse(self.constV);

        return new CmdRes()
        {
            v = new List<VarForm.Data>() { new VarForm.Data(-1, "", 7, 0, null, jo.Get<List<ClipForm.Data>>(ValueType.Clips.ToString()), 0)

            }
        };
    }

}
