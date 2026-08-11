using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Z_Code.Form;
using Z_Debug;

namespace Z_Code
{
    public class LexicalNode : Node
    {
        public LexicalNode(string code, int startIndex = -1)
        {
            rawCode = code;
            this.startIndex = startIndex;
        }

        public string rawCode;
        public int startIndex = -1;
    }

    public class Desc
    {
        public Desc(string code, CodeType type, int codeIndex = -1)
        {
            this.type = type;
            this.code = code;
            this.codeIndex = codeIndex;
        }

        public string code;
        public CodeType type;
        public int codeIndex = -1;

        public string retType
        {
            get
            {
                switch (type)
                {
                    case CodeType.Num:
                        return "num";
                    case CodeType.VarName:
                        return "var";
                    case CodeType.Str:
                        return "string";
                    case CodeType.FuncName:
                    case CodeType.Operator:
                        if (CmdDataForm.DataByName.TryGetValue(code, out var directForm))
                        {
                            return GetFirstReturnType(directForm);
                        }
                        if (BaseData.cmdDic.TryGetValue(code, out var aliasCmd) &&
                            CmdDataForm.DataByName.TryGetValue(aliasCmd.GetName(), out var canonicalForm))
                        {
                            return GetFirstReturnType(canonicalForm);
                        }
                        if (ProgramDataForm.DataByName.TryGetValue(code, out var program))
                        {
                            return string.IsNullOrEmpty(program.returnValue) ? "var" : program.returnValue;
                        }
                        switch (code)
                        {
                            case "%":
                            case "!":
                            case "&&":
                            case "||":
                                return "num";
                            default:
                                return "var";
                        }
                    default:
                        return "null";
                }
            }
        }

        private static string GetFirstReturnType(CmdDataForm.Data form)
        {
            if (form?.retTypes == null || form.retTypes.Count == 0 || string.IsNullOrEmpty(form.retTypes[0]))
            {
                return "void";
            }
            return form.retTypes[0];
        }
    }

    public class LexicalAnalysis
    {
        public static bool DEBUG = false;

        private static readonly string[] CompoundOperators =
        {
            "==", "!=", ">=", "<=", "&&", "||"
        };

        private string _originalCode;
        private List<CompileError> _errors;

        public List<LexicalNode> Execute(string code, List<CompileError> errors = null)
        {
            code = code ?? string.Empty;
            _originalCode = code;
            _errors = errors ?? new List<CompileError>();

            List<LexicalNode> nodes;
            try
            {
                nodes = ManageString(code);
            }
            catch (Exception ex)
            {
                AddError(0, Math.Max(0, code.Length - 1), "词法分析", $"拆分 Token 失败: {ex.Message}", ex);
                return new List<LexicalNode>();
            }

            try
            {
                ManageDesc(nodes);
            }
            catch (Exception ex)
            {
                AddError(0, Math.Max(0, code.Length - 1), "词法分析", $"Token 分类失败: {ex.Message}", ex);
            }

            if (DEBUG || Compiler.DEBUG)
            {
                Z_Log.Log(nodes, new[] { "desc.code", "desc.type" });
            }
            return nodes;
        }

        public List<LexicalNode> ManageString(string code)
        {
            var nodes = new List<LexicalNode>();
            if (string.IsNullOrEmpty(code))
            {
                return nodes;
            }

            int index = 0;
            while (index < code.Length)
            {
                char current = code[index];
                if (IsEmpty(current))
                {
                    index++;
                    continue;
                }

                if (IsString(current))
                {
                    int start = index;
                    char quote = current;
                    index++;
                    bool closed = false;
                    while (index < code.Length)
                    {
                        if (code[index] == '\\')
                        {
                            index += Math.Min(2, code.Length - index);
                            continue;
                        }
                        if (code[index] == quote)
                        {
                            index++;
                            closed = true;
                            break;
                        }
                        index++;
                    }

                    nodes.Add(new LexicalNode(code.Substring(start, index - start), start));
                    if (!closed)
                    {
                        AddError(start, Math.Max(start, code.Length - 1), "词法分析", "字符串缺少结束引号");
                    }
                    continue;
                }

                if (IsSplit(current))
                {
                    nodes.Add(new LexicalNode(current.ToString(), index));
                    index++;
                    continue;
                }

                if (TryReadOperator(code, index, out var op))
                {
                    nodes.Add(new LexicalNode(op, index));
                    index += op.Length;
                    continue;
                }

                int tokenStart = index;
                if (IsNum(current))
                {
                    bool hasDot = false;
                    while (index < code.Length)
                    {
                        if (IsNum(code[index]))
                        {
                            index++;
                            continue;
                        }
                        if (!hasDot && code[index] == '.')
                        {
                            hasDot = true;
                            index++;
                            continue;
                        }
                        break;
                    }
                }
                else
                {
                    while (index < code.Length &&
                           !IsEmpty(code[index]) &&
                           !IsString(code[index]) &&
                           !IsSplit(code[index]) &&
                           !TryReadOperator(code, index, out _))
                    {
                        index++;
                    }
                }

                if (index == tokenStart)
                {
                    AddError(index, index, "词法分析", $"无法识别字符 '{code[index]}'");
                    index++;
                    continue;
                }
                nodes.Add(new LexicalNode(code.Substring(tokenStart, index - tokenStart), tokenStart));
            }

            return nodes;
        }

        public void ManageDesc(List<LexicalNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                try
                {
                    if (IsString(node))
                    {
                        node.desc = new Desc(DecodeString(node), CodeType.Str, node.startIndex);
                    }
                    else if (IsNum(node.rawCode))
                    {
                        node.desc = new Desc(node.rawCode, CodeType.Num, node.startIndex);
                    }
                    else if (IsSplit(node.rawCode))
                    {
                        node.desc = new Desc(node.rawCode, CodeType.Split, node.startIndex);
                    }
                    else if (IsOperator(node.rawCode))
                    {
                        node.desc = new Desc(node.rawCode, CodeType.Operator, node.startIndex);
                    }
                    else if (IsReserved(node.rawCode))
                    {
                        node.desc = new Desc(node.rawCode, CodeType.Reserved, node.startIndex);
                    }
                    else if (IsCmd(node.rawCode) || (i + 1 < nodes.Count && nodes[i + 1].rawCode == "("))
                    {
                        node.desc = new Desc(node.rawCode, CodeType.FuncName, node.startIndex);
                    }
                    else
                    {
                        node.desc = new Desc(node.rawCode, CodeType.VarName, node.startIndex);
                    }
                }
                catch (Exception ex)
                {
                    AddError(node.startIndex, node.startIndex + Math.Max(0, node.rawCode.Length - 1),
                        "词法分析", $"Token '{node.rawCode}' 分类失败: {ex.Message}", ex);
                    node.desc = new Desc(node.rawCode, CodeType.VarName, node.startIndex);
                }
            }
        }

        private string DecodeString(LexicalNode node)
        {
            string raw = node.rawCode;
            if (raw.Length < 2 || raw[raw.Length - 1] != raw[0])
            {
                return raw.Length > 1 ? raw.Substring(1) : string.Empty;
            }

            var result = new StringBuilder();
            for (int i = 1; i < raw.Length - 1; i++)
            {
                char ch = raw[i];
                if (ch != '\\')
                {
                    result.Append(ch);
                    continue;
                }

                if (++i >= raw.Length - 1)
                {
                    AddError(node.startIndex + i - 1, node.startIndex + i - 1,
                        "词法分析", "字符串末尾存在不完整转义");
                    break;
                }

                switch (raw[i])
                {
                    case '\\': result.Append('\\'); break;
                    case '"': result.Append('"'); break;
                    case '\'': result.Append('\''); break;
                    case 'n': result.Append('\n'); break;
                    case 'r': result.Append('\r'); break;
                    case 't': result.Append('\t'); break;
                    case '0': result.Append('\0'); break;
                    case 'b': result.Append('\b'); break;
                    case 'f': result.Append('\f'); break;
                    case 'u':
                        if (i + 4 < raw.Length - 1 &&
                            int.TryParse(raw.Substring(i + 1, 4), NumberStyles.HexNumber,
                                CultureInfo.InvariantCulture, out int unicode))
                        {
                            result.Append((char)unicode);
                            i += 4;
                        }
                        else
                        {
                            AddError(node.startIndex + i - 1, node.startIndex + i,
                                "词法分析", "无效的 Unicode 转义");
                            result.Append('u');
                        }
                        break;
                    default:
                        AddError(node.startIndex + i - 1, node.startIndex + i,
                            "词法分析", $"不支持的转义 \\{raw[i]}");
                        result.Append(raw[i]);
                        break;
                }
            }
            return result.ToString();
        }

        private static bool TryReadOperator(string code, int index, out string op)
        {
            foreach (var candidate in CompoundOperators)
            {
                if (index + candidate.Length <= code.Length &&
                    string.CompareOrdinal(code, index, candidate, 0, candidate.Length) == 0)
                {
                    op = candidate;
                    return true;
                }
            }

            string single = code[index].ToString();
            if (BaseData.operators.Contains(single))
            {
                op = single;
                return true;
            }

            op = null;
            return false;
        }

        private void AddError(int startIndex, int endIndex, string stage, string message, Exception ex = null)
        {
            var (line, column) = GetLineAndColumn(startIndex);
            _errors?.Add(new CompileError
            {
                StartIndex = startIndex,
                EndIndex = endIndex,
                LineNumber = line,
                ColumnNumber = column,
                Stage = stage,
                Message = message,
                Exception = ex
            });
        }

        private (int line, int column) GetLineAndColumn(int index)
        {
            if (string.IsNullOrEmpty(_originalCode) || index < 0)
            {
                return (1, 1);
            }

            int line = 1;
            int column = 1;
            int maxIndex = Math.Min(index, _originalCode.Length);
            for (int i = 0; i < maxIndex; i++)
            {
                if (_originalCode[i] == '\n')
                {
                    line++;
                    column = 1;
                }
                else
                {
                    column++;
                }
            }
            return (line, column);
        }

        public static bool IsNum(char ch)
        {
            return ch >= '0' && ch <= '9';
        }

        public static bool IsEmpty(char ch)
        {
            return ch == '\0' || char.IsWhiteSpace(ch);
        }

        public static bool IsSplit(char ch)
        {
            return ch == ';';
        }

        public static bool IsSplit(string str)
        {
            return str == ";";
        }

        public static bool IsNum(string str)
        {
            if (string.IsNullOrEmpty(str) || str[0] == '.')
            {
                return false;
            }

            bool hasDot = false;
            foreach (var ch in str)
            {
                if (IsNum(ch))
                {
                    continue;
                }
                if (!hasDot && ch == '.')
                {
                    hasDot = true;
                    continue;
                }
                return false;
            }
            return true;
        }

        public static bool IsOperator(char ch)
        {
            return BaseData.operators.Any(op => op.Length > 0 && op[0] == ch);
        }

        public static bool IsOperator(string str)
        {
            return BaseData.operators.Contains(str);
        }

        public static bool IsFirstOperator(string str)
        {
            switch (str)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "%":
                case "=":
                case "!":
                case ">":
                case "<":
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsReserved(string str)
        {
            return BaseData.reserved.Contains(str);
        }

        public static bool IsCmd(string str)
        {
            return BaseData.cmdDic.ContainsKey(str);
        }

        public static bool IsString(char ch)
        {
            return ch == '"' || ch == '\'';
        }

        public static bool IsString(LexicalNode node)
        {
            return node != null && !string.IsNullOrEmpty(node.rawCode) && IsString(node.rawCode[0]);
        }
    }
}
