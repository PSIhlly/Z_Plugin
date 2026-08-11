using System.Collections.Generic;
using System.Text;
using Z_Debug;

namespace Z_Code
{
    public class Decompiler
    {
        public static bool DEBUG = false;

        public string Decompile(List<SyntaxNode> syntaxNodes)
        {
            var result = new StringBuilder();
            if (syntaxNodes != null)
            {
                foreach (var node in syntaxNodes)
                {
                    result.Append(RenderStatement(node, 0));
                    if (result.Length > 0 && result[result.Length - 1] != '\n')
                    {
                        result.Append('\n');
                    }
                }
            }

            string code = result.ToString();
            if (DEBUG || Compiler.DEBUG)
            {
                Z_Log.Log(code);
            }
            return code;
        }

        /// <summary>
        /// Returns a node without forcing a trailing statement semicolon. Kept for visual-editor callers.
        /// </summary>
        public string ResetStatement(SyntaxNode node)
        {
            if (node == null)
            {
                return string.Empty;
            }
            if (node.desc.type == CodeType.Action)
            {
                return RenderAction(node, 0);
            }
            if (node.desc.type == CodeType.Reserved)
            {
                return RenderReserved(node, 0, false);
            }
            return RenderExpression(node);
        }

        private string RenderStatement(SyntaxNode node, int indent)
        {
            if (node == null || SyntaxAnalysis.IsEmptyNode(node))
            {
                return string.Empty;
            }
            if (node.desc.type == CodeType.Action)
            {
                return RenderAction(node, indent);
            }
            if (node.desc.type == CodeType.Reserved)
            {
                return RenderReserved(node, indent, true);
            }
            return Indent(indent) + RenderExpression(node) + ";\n";
        }

        private string RenderReserved(SyntaxNode node, int indent, bool terminateSimple)
        {
            string prefix = Indent(indent);
            string suffix = terminateSimple ? ";\n" : string.Empty;
            switch (node.desc.code)
            {
                case "Wait":
                    return prefix + "Wait(" + RenderChild(node, 0) + ")" + suffix;
                case "Return":
                    return prefix + "Return" + (node.subNodes.Count == 0 ? string.Empty : " " + RenderChild(node, 0)) + suffix;
                case "break":
                    return prefix + "break" + suffix;
                case "continue":
                    return prefix + "continue" + suffix;
                case "if":
                {
                    var result = new StringBuilder();
                    result.Append(prefix).Append("if (").Append(RenderChild(node, 0)).Append(")\n");
                    result.Append(prefix).Append("{\n");
                    result.Append(node.subNodes.Count > 1 ? RenderAction(node.subNodes[1], indent + 1) : string.Empty);
                    result.Append(prefix).Append('}');
                    if (node.subNodes.Count > 2)
                    {
                        result.Append("\n").Append(prefix).Append("else\n");
                        result.Append(prefix).Append("{\n");
                        result.Append(RenderAction(node.subNodes[2], indent + 1));
                        result.Append(prefix).Append('}');
                    }
                    if (terminateSimple)
                    {
                        result.Append('\n');
                    }
                    return result.ToString();
                }
                case "while":
                {
                    var result = new StringBuilder();
                    result.Append(prefix).Append("while (").Append(RenderChild(node, 0)).Append(")\n");
                    result.Append(prefix).Append("{\n");
                    result.Append(node.subNodes.Count > 1 ? RenderAction(node.subNodes[1], indent + 1) : string.Empty);
                    result.Append(prefix).Append('}');
                    if (terminateSimple)
                    {
                        result.Append('\n');
                    }
                    return result.ToString();
                }
                case "for":
                {
                    string init = RenderForClause(node, 0);
                    string condition = RenderForClause(node, 1);
                    string iteration = RenderForClause(node, 2);
                    var result = new StringBuilder();
                    result.Append(prefix).Append("for (").Append(init).Append(';')
                        .Append(condition).Append(';').Append(iteration).Append(")\n");
                    result.Append(prefix).Append("{\n");
                    result.Append(node.subNodes.Count > 3 ? RenderAction(node.subNodes[3], indent + 1) : string.Empty);
                    result.Append(prefix).Append('}');
                    if (terminateSimple)
                    {
                        result.Append('\n');
                    }
                    return result.ToString();
                }
                default:
                    return prefix + node.desc.code + suffix;
            }
        }

        private string RenderAction(SyntaxNode action, int indent)
        {
            if (action == null || SyntaxAnalysis.IsEmptyNode(action))
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            foreach (var child in action.subNodes)
            {
                result.Append(RenderStatement(child, indent));
            }
            return result.ToString();
        }

        private string RenderForClause(SyntaxNode node, int index)
        {
            if (node.subNodes.Count <= index || SyntaxAnalysis.IsEmptyNode(node.subNodes[index]))
            {
                return string.Empty;
            }
            return RenderExpression(node.subNodes[index]);
        }

        private string RenderChild(SyntaxNode node, int index)
        {
            return node.subNodes.Count > index ? RenderExpression(node.subNodes[index]) : string.Empty;
        }

        private string RenderExpression(SyntaxNode node)
        {
            if (node == null || SyntaxAnalysis.IsEmptyNode(node))
            {
                return string.Empty;
            }

            switch (node.desc.type)
            {
                case CodeType.Num:
                case CodeType.VarName:
                    return node.desc.code;
                case CodeType.Str:
                    return "\"" + EscapeString(node.desc.code) + "\"";
                case CodeType.FuncName:
                {
                    var args = new StringBuilder();
                    for (int i = 0; i < node.subNodes.Count; i++)
                    {
                        if (i > 0)
                        {
                            args.Append(',');
                        }
                        args.Append(RenderExpression(node.subNodes[i]));
                    }
                    return node.desc.code + "(" + args + ")";
                }
                case CodeType.Operator:
                    return RenderOperator(node);
                case CodeType.Action:
                    return RenderAction(node, 0);
                case CodeType.Reserved:
                    return RenderReserved(node, 0, false);
                default:
                    return node.desc.code ?? string.Empty;
            }
        }

        private string RenderOperator(SyntaxNode node)
        {
            if (node.subNodes.Count == 1)
            {
                return node.desc.code + "(" + RenderExpression(node.subNodes[0]) + ")";
            }
            if (node.subNodes.Count < 2)
            {
                return node.desc.code;
            }

            string right = RenderExpression(node.subNodes[0]);
            string left = RenderExpression(node.subNodes[1]);
            switch (node.desc.code)
            {
                case ".":
                    return left + "." + right;
                case "[":
                    return left + "[" + right + "]";
                default:
                    // Full parenthesization guarantees round-trip semantics for all precedence levels.
                    return "(" + left + node.desc.code + right + ")";
            }
        }

        private static string EscapeString(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder();
            foreach (char ch in value)
            {
                switch (ch)
                {
                    case '\\': result.Append("\\\\"); break;
                    case '"': result.Append("\\\""); break;
                    case '\n': result.Append("\\n"); break;
                    case '\r': result.Append("\\r"); break;
                    case '\t': result.Append("\\t"); break;
                    case '\0': result.Append("\\0"); break;
                    case '\b': result.Append("\\b"); break;
                    case '\f': result.Append("\\f"); break;
                    default:
                        if (char.IsControl(ch))
                        {
                            result.Append("\\u").Append(((int)ch).ToString("X4"));
                        }
                        else
                        {
                            result.Append(ch);
                        }
                        break;
                }
            }
            return result.ToString();
        }

        private static string Indent(int level)
        {
            return new string(' ', level * 4);
        }
    }
}
