using Codice.CM.Common;
using System;
using System.Collections.Generic;
using Z_Code;
using Z_Code.Form;
namespace Z_Code.Form
{
    public static partial class BoxDataForm
    {
        public partial class Data
        {
            public Data DeepCopy(bool sameId = true)
            {
                var tmpDic = new Dictionary<string, BoxDataForm.Data>();
                foreach (var pair in dic)
                {
                    tmpDic[pair.Key] = pair.Value.DeepCopy();
                }
                return new Data(sameId ? uid : uidChain.GetId(), str, valName, num, tmpDic);
            }
        }
    }
    public static partial class ProgramDataForm
    {
        public partial class Data
        {
            public List<SyntaxNode> ApplyCode(string code, Compiler compiler = null)
            {
                if (compiler == null)
                    compiler = new Compiler();
                this.code = code;
                zCode = compiler.Compile(code, out var res, out int count, out string ret);
                paramCount = count;
                returnValue = ret;
                return res;
            }
        }
    }
}
namespace Z_Code
{

    public static class CodeHelper
    {
        public static BoxDataForm.Data CreateBox()
        {
            return new BoxDataForm.Data(-1, null, null, 0, new Dictionary<string, BoxDataForm.Data>());
        }
        public static BoxDataForm.Data CreateBoxByVal(string valName)
        {
            return new BoxDataForm.Data(-1, null, valName, 0, new Dictionary<string, BoxDataForm.Data>());
        }

        public static BoxDataForm.Data CreateBoxByStr(string str)
        {
            return new BoxDataForm.Data(-1, str, null, 0, new Dictionary<string, BoxDataForm.Data>());
        }
        public static BoxDataForm.Data CreateBoxByNum(float num)
        {
            return new BoxDataForm.Data(-1, null, null, num, new Dictionary<string, BoxDataForm.Data>());
        }
        public static string GetBoxContent(this BoxDataForm.Data data)
        {
            if (data.str == null)
            {
                return data.num.ToString();
            }
            return data.str;
        }

    }

}