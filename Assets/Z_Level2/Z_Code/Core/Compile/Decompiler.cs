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
                    AppendBlock(result, node.subNodes.Count > 1 ? node.subNodes[1] : null, indent);
                    if (node.subNodes.Count > 2)
                    {
                        result.Append("\n").Append(prefix).Append("else\n");
                        AppendBlock(result, node.subNodes[2], indent);
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
                    AppendBlock(result, node.subNodes.Count > 1 ? node.subNodes[1] : null, indent);
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
                        .Append(condition.Length > 0 ? " " : string.Empty).Append(condition).Append(';')
                        .Append(iteration.Length > 0 ? " " : string.Empty).Append(iteration).Append(")\n");
                    AppendBlock(result, node.subNodes.Count > 3 ? node.subNodes[3] : null, indent);
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

        private void AppendBlock(StringBuilder result, SyntaxNode body, int indent)
        {
            result.Append(Indent(indent)).Append("{\n");
            result.Append(RenderAction(body, indent + 1));
            result.Append(Indent(indent)).Append('}');
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
                            args.Append(", ");
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
            if (node.desc.code == "++" && node.subNodes.Count == 1)
            {
                return RenderOperand(node.subNodes[0], GetPrecedence(node), false) + "++";
            }
            if (node.subNodes.Count == 1)
            {
                string operand = RenderOperand(node.subNodes[0], GetPrecedence(node), false);
                // Keep adjacent unary plus operators separate from the postfix ++ token.
                if (node.desc.code == "+" && operand.StartsWith("+"))
                    operand = " " + operand;
                return node.desc.code + operand;
            }
            if (node.subNodes.Count < 2)
            {
                return node.desc.code;
            }

            int precedence = GetPrecedence(node);
            if (node.desc.code == "[")
                return RenderOperand(node.subNodes[1], precedence, false)
                    + "[" + RenderExpression(node.subNodes[0]) + "]";

            bool rightAssociative = node.desc.code == "=" || node.desc.code == "+=";
            string right = RenderOperand(node.subNodes[0], precedence, !rightAssociative);
            string left = RenderOperand(node.subNodes[1], precedence, rightAssociative);
            switch (node.desc.code)
            {
                case ".":
                    // The lexer consumes the dot in "1.member" as part of a number.
                    if (node.subNodes[1].desc.type == CodeType.Num)
                        left = "(" + left + ")";
                    return left + "." + right;
                default:
                    return left + " " + node.desc.code + " " + right;
            }
        }

        private string RenderOperand(SyntaxNode node, int parentPrecedence, bool parenthesizeOnEqual)
        {
            string expression = RenderExpression(node);
            if (node?.desc?.type != CodeType.Operator)
                return expression;

            int precedence = GetPrecedence(node);
            return precedence < parentPrecedence || (precedence == parentPrecedence && parenthesizeOnEqual)
                ? "(" + expression + ")"
                : expression;
        }

        // Mirrors SyntaxAnalysis.ParseAssignment through ParsePostfix. Equal-precedence
        // right operands stay grouped for left-associative operators (including + and *:
        // reassociation can change floating-point or string results).
        private static int GetPrecedence(SyntaxNode node)
        {
            if (node?.desc?.type != CodeType.Operator)
                return 10;
            if (node.subNodes.Count == 1)
                return node.desc.code == "++" ? 9 : 8;

            switch (node.desc.code)
            {
                case "=":
                case "+=": return 1;
                case "||": return 2;
                case "&&": return 3;
                case "==":
                case "!=": return 4;
                case ">":
                case "<":
                case ">=":
                case "<=": return 5;
                case "+":
                case "-": return 6;
                case "*":
                case "/":
                case "%": return 7;
                case ".":
                case "[": return 9;
                default: return 0;
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
