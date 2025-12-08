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
            "Wait"
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

        public List<string> Compile(string code, out List<SyntaxNode> syntaxs)
        {
            var lexicals = lexicalAnalysis.Execute(code);
            syntaxs = syntaxAnalysis.Execute(lexicals);
            var zl = zLanguageAnalysis.Execute(syntaxs);
            if (DEBUG)
            {
                Z_Log.Log(zl);
            }
            return zl;
        }

    }
}
