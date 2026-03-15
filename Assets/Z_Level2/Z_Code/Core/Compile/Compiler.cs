using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Code.Form;
using Z_Debug;

namespace Z_Code
{
    public enum CodeType
    {
        Num,
        Str,
        VarName,
        FuncName,
        Action,
        Reserved,
        Operator,
        Split,
    }

    public abstract class Node
    {

        public Desc desc;
    }
    public static class BaseData
    {
        public static CmdDataForm.Data GetForm(this CmdBase cmd)
        {
            return CmdDataForm.DataByName[cmd.GetName()];
        }

        public static Dictionary<string, CmdBase> cmdDic = new Dictionary<string, CmdBase>();

        public static HashSet<string> reserved = new HashSet<string>()
        {
            "if",
            "for",
            "else",
            "Wait",
            "Return"
        };
        public static HashSet<string> operators = new HashSet<string>()
        {
            "+",
            "-",
            "*",
            "/",
            "=",
            "(",
            ")",
            "[",
            "]",
            "%",
            "{",
            "}",
            ".",
            ",",
            "==",
            "!=",
            "!",
            ">",
            "<",
            ">=",
            "<="
        };
    }
    public class Compiler
    {
        public static bool DEBUG;
        LexicalAnalysis lexicalAnalysis = new LexicalAnalysis();
        SyntaxAnalysis syntaxAnalysis = new SyntaxAnalysis();
        ZLanguageAnalysis zLanguageAnalysis = new ZLanguageAnalysis();

        public List<string> Compile(string code, out List<SyntaxNode> syntaxs,out int paramCount,out string ret)
        {
            var lexicals = lexicalAnalysis.Execute(code);
            syntaxs = syntaxAnalysis.Execute(lexicals);
            var zl = zLanguageAnalysis.Execute(syntaxs);


            var retRes= "void";
            var paramCountRes = 0;
            foreach (var node in syntaxs)
            {
                DfsNode((o) =>
                {
                    if (o.desc.type == CodeType.VarName)
                    {
                        var splits = o.desc.code.Split("param");
                        if (splits.Length == 2 && string.IsNullOrEmpty(splits[0]) && int.TryParse(splits[1], out int id))
                        {
                            paramCountRes = Math.Max(id, paramCountRes);
                        }
                    }
                    else if (o.desc.type == CodeType.Reserved && node.desc.code == "Return")
                    {
                        retRes = o.subNodes[0].desc.code;
                    }
                }, node);
                
            }
            ret = retRes;
            paramCount = paramCountRes;
            if (DEBUG)
            {
                Z_Log.Log(zl);
            }
            return zl;
        }
        public void DfsNode(Action<SyntaxNode> manage, SyntaxNode node)
        {
            manage(node);
            foreach(var sub in node.subNodes)
            {
                DfsNode(manage,sub);
            }

        }

    }
}
