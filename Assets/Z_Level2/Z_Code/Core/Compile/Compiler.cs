using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
    public static class BaseData
    {
  

        public static Dictionary<string, CmdBase> cmdDic = new Dictionary<string, CmdBase>();
        public static HashSet<string> reserved = new HashSet<string>()
        {
            "if",
            "for",

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
            "%",
            "{",
            "}",
            ".",
            ",",
        };
    }
    public class Compiler
    {
        LexicalAnalysis lexicalAnalysis = new LexicalAnalysis();
        SyntaxAnalysis syntaxAnalysis = new SyntaxAnalysis();
        ZLanguageAnalysis zLanguageAnalysis = new ZLanguageAnalysis();

        public List<string> Compile(string code)
        {
            var lexicals = lexicalAnalysis.Execute(code);
            var syntaxs = syntaxAnalysis.Execute(lexicals);
            var zl = zLanguageAnalysis.Execute(syntaxs);
            return zl;
        }
    }
}
