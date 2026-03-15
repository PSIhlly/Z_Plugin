using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_Debug;

namespace Z_Code
{
    public class Decompiler
    {
        public static bool DEBUG = true;
        public string Decompile(List<SyntaxNode> syntaxNodes)
        {
            string code = "";
            foreach (var node in syntaxNodes)
            {
                code += ResetStatement(node) + ";\n";
            }
            if (DEBUG)
            {
                Z_Log.Log(code);
            }
            return code;
        }
        public string ResetStatement(SyntaxNode node)
        {
            var code = "";
            if (node.desc.type == CodeType.Action)
            {
                foreach (var sub in node.subNodes)
                {
                    code += ResetStatement(sub) + ";\n";
                }
            }
            if (node.desc.type == CodeType.Reserved)
            {
                switch (node.desc.code)
                {
                    case "Wait":
                        code = $"Wait({ResetStatement(node.subNodes[0])})";
                        break;
                    case "Return":
                        code = $"Return {ResetStatement(node.subNodes[0])}";
                        break;
                    case "if":
                        code = $"if({ResetStatement(node.subNodes[0])})\n{{\n{ResetStatement(node.subNodes[1])}}} \n" +
                            $"{(node.subNodes.Count > 2 ? $"else \n{{\n{ResetStatement(node.subNodes[2])}}}\n" : "")}";
                        break;
                    case "for":
                        code = $"for({ResetStatement(node.subNodes[0])};{ResetStatement(node.subNodes[1])};{ResetStatement(node.subNodes[2])})\n{{\n{ResetStatement(node.subNodes[3])} }}\n ";
                        break;
                }
            }



            //level1
            if (node.desc.type == CodeType.FuncName)
            {
                for (int i = 0; i < node.subNodes.Count; i++)
                {
                    code += ResetStatement(node.subNodes[i]);
                    if (i < node.subNodes.Count - 1)
                    {
                        code += ",";
                    }
                }
                code = $"{node.desc.code}({code})";
            }
            //level2

            if (node.desc.type == CodeType.Operator)
            {
                switch (node.desc.code)
                {
                    case "*":
                    case "/":
                        code = $"({ResetStatement(node.subNodes[1])}){node.desc.code}({ResetStatement(node.subNodes[0])})";
                        break;
                    case "+":
                    case "-":
                    case ".":
                    case "=":
                    case "==":
                    case "!=":
                    case ">":
                    case "<":
                    case ">=":
                    case "<=":
                        if(node.subNodes.Count > 1)
                        {
                            code = $"{ResetStatement(node.subNodes[1])}{node.desc.code}{ResetStatement(node.subNodes[0])}";
                        }else
                        {
                            code = $"{node.desc.code}{ResetStatement(node.subNodes[0])}";
                        }
                            break;
                    case "[":
                        code = $"{ResetStatement(node.subNodes[1])}[{ResetStatement(node.subNodes[0])}]";
                        break;

                }
            }
            switch (node.desc.type)
            {
                case CodeType.Num:
                    code = $"{node.desc.code}";
                    break;
                case CodeType.Str:
                    code = $"\"{node.desc.code}\"";
                    break;
                case CodeType.VarName:
                    code = $"{node.desc.code}";
                    break;
            }

            return code;
        }
    }
}
