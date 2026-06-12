using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_Code.Form;
using Z_Debug;

namespace Z_Code
{

    public class LexicalNode : Node
    {
        public LexicalNode(string code, int startIndex = -1)
        {
            this.rawCode = code;
            this.startIndex = startIndex;
        }
        public string rawCode;
        /// <summary>
        /// 在原始代码中的起始位置（字符索引）
        /// </summary>
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
        /// <summary>
        /// 在原始代码中的起始位置（字符索引）
        /// </summary>
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
                    case CodeType.FuncName:
                    case CodeType.Operator:
                        return CmdDataForm.DataByName[code].retTypes[0];
                    case CodeType.Str:
                        return "string";
                    default:
                        return "null";
                }
            }

        }
    }
    public class LexicalAnalysis
    {
        public const bool DEBUG = true;

        /// <summary>
        /// 原始代码（用于计算错误位置）
        /// </summary>
        private string _originalCode;

        /// <summary>
        /// 错误列表
        /// </summary>
        private List<CompileError> _errors;

        public List<LexicalNode> Execute(string code, List<CompileError> errors = null)
        {
            _originalCode = code;
            _errors = errors ?? new List<CompileError>();

            List<LexicalNode> lst = null;
            try
            {
                lst = ManageString(code);
            }
            catch (Exception ex)
            {
                AddError(0, code.Length - 1, "词法分析", $"字符串解析失败: {ex.Message}", ex);
                return new List<LexicalNode>();
            }

            try
            {
                ManageDesc(lst);
            }
            catch (Exception ex)
            {
                AddError(0, code.Length - 1, "词法分析", $"描述解析失败: {ex.Message}", ex);
            }

            if (DEBUG)
            {
                Z_Log.Log(lst, new[] { "desc.code", "desc.type" });
            }
            return lst;
        }

        /// <summary>
        /// 添加错误
        /// </summary>
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

        /// <summary>
        /// 根据字符索引计算行号和列号
        /// </summary>
        private (int line, int column) GetLineAndColumn(int index)
        {
            if (string.IsNullOrEmpty(_originalCode) || index < 0)
            {
                return (1, 1);
            }

            int line = 1;
            int column = 1;
            int maxIndex = Math.Min(index, _originalCode.Length - 1);

            for (int i = 0; i <= maxIndex; i++)
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
        public List<LexicalNode> ManageString(string code)
        {
            StringBuilder sb = new StringBuilder();
            List<LexicalNode> lst = new List<LexicalNode>();
            bool isStringNow = false;
            for (int i = 0, icnt = code.Length; i < icnt; i++)
            {
                try
                {
                    if (IsString(code[i]))
                    {
                        isStringNow = !isStringNow;
                        if (!isStringNow)
                        {
                            sb.Append(code[i]);
                            End(lst, sb, i + 1);
                        }
                        else
                        {
                            End(lst, sb, i);
                            sb.Append(code[i]);
                        }
                    }
                    else if (isStringNow)
                    {
                        sb.Append(code[i]);
                    }
                    else if (IsEmpty(code[i]))
                    {
                        End(lst, sb, i);
                    }
                    else if (IsSplit(code[i]))
                    {
                        End(lst, sb, i);
                        if (code[i] == ';')
                        {
                            sb.Append(code[i]);
                            End(lst, sb, i + 1);
                        }
                    }
                    else if (IsNum(code[i]))
                    {
                        if (IsOperator(sb.ToString()))
                        {
                            End(lst, sb, i);
                        }
                        sb.Append(code[i]);
                    }
                    else if (IsOperator(code[i]))
                    {
                        if (IsNum(sb.ToString()))
                        {
                            if (!IsNum(sb.ToString() + code[i]))
                                End(lst, sb, i);
                        }
                        else if (!IsOperator(sb.ToString() + code[i]))
                        {
                            End(lst, sb, i);
                        }
                        sb.Append(code[i]);
                    }
                    else
                    {
                        if (IsNum(sb.ToString()) || IsOperator(sb.ToString()))
                        {
                            End(lst, sb, i);
                            sb.Append(code[i]);
                        }
                        else
                        {
                            sb.Append(code[i]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddError(i, i, "词法分析", $"处理字符 '{code[i]}' 时出错: {ex.Message}", ex);
                }
            }
            End(lst, sb, code.Length);


            return lst;


        }
        public void ManageDesc(List<LexicalNode> nodes)
        {
            for (int i = 0, icnt = nodes.Count; i < icnt; i++)
            {
                try
                {
                    if (IsString(nodes[i]))
                    {
                        nodes[i].desc = new Desc(nodes[i].rawCode.Substring(1, nodes[i].rawCode.Length - 2), CodeType.Str, nodes[i].startIndex);
                    }
                    else if (IsNum(nodes[i].rawCode))
                    {
                        nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.Num, nodes[i].startIndex);
                    }
                    else
                    {
                        if (IsSplit(nodes[i].rawCode))
                        {
                            nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.Split, nodes[i].startIndex);
                        }
                        else if (IsOperator(nodes[i].rawCode))
                        {
                            nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.Operator, nodes[i].startIndex);
                        }
                        else if (IsReserved(nodes[i].rawCode))
                        {
                            nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.Reserved, nodes[i].startIndex);
                        }
                        else if (IsCmd(nodes[i].rawCode) || (i + 1 < icnt && nodes[i + 1].rawCode == "("))
                        {
                            nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.FuncName, nodes[i].startIndex);
                        }
                        else
                        {
                            nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.VarName, nodes[i].startIndex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddError(0, _originalCode.Length - 1, "词法分析", $"处理词法节点 '{nodes[i].rawCode}' 时出错: {ex.Message}", ex);
                }
            }
        }


        private bool End(List<LexicalNode> lst, StringBuilder sb, int currentIndex = -1)
        {
            if (sb.Length == 0)
            {
                return false;
            }
            else
            {
                int startIndex = currentIndex >= 0 ? currentIndex - sb.Length : -1;
                lst.Add(new LexicalNode(sb.ToString(), startIndex));
                sb.Clear();
                return true;
            }

        }

        #region str
        public static bool IsNum(char ch)
        {
            return ch >= '0' && ch <= '9';
        }
        public static bool IsEmpty(char ch)
        {
            switch (ch)
            {
                case ' ':
                case '\n':
                case '\0':
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsSplit(char ch)
        {
            switch (ch)
            {
                case ';':
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsSplit(string str)
        {
            switch (str)
            {
                case ";":
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsNum(string str)
        {
            if (str.StartsWith('.'))
                return false;
            bool hasDot = false;
            foreach (var ch in str)
            {
                if (!IsNum(ch))
                {
                    if (hasDot)
                        return false;
                    if (ch == '.')
                    {
                        hasDot = true;
                        continue;
                    }
                    return false;
                }
            }
            return true;
        }
        public static bool IsOperator(char ch)
        {
            return BaseData.operators.Contains(ch.ToString());
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
            switch (ch)
            {
                case '\"':
                case '\'': return true;
                default:
                    return false;
            }
        }
        #endregion

        #region lexi
        public static bool IsString(LexicalNode node)
        {
            return node.rawCode.StartsWith("\"") || node.rawCode.StartsWith("\'");
        }
        #endregion
    }
}
