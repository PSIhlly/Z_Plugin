using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_Debug;

namespace Z_Code
{

    public class LexicalNode: Node
    {
        public LexicalNode(string code)
        {
            this.rawCode = code;
        }
        public string rawCode;
        public Desc desc;
    }
    public class Desc
    {
        public Desc(string code, CodeType type)
        {
            this.type = type;
            this.code = code;
        }
        public string code;
        public CodeType type;
    }
    public class LexicalAnalysis
    {
        public const bool DEBUG = true;

        public List<LexicalNode> Execute(string code)
        {
            var lst=ManageString(code);
            ManageDesc(lst);
            if (DEBUG)
            {
                Z_Log.Log(lst, new[] { "desc.code", "desc.type" });
            }
            return lst;
        }
            public List<LexicalNode> ManageString(string code)
        {
            StringBuilder sb = new StringBuilder();
            List<LexicalNode> lst = new List<LexicalNode>();
            bool isStringNow=false;
            for (int i = 0, icnt = code.Length; i < icnt; i++)
            {

                if (IsString(code[i]))
                {
                    isStringNow = !isStringNow;
                    if(!isStringNow)
                    {
                        sb.Append(code[i]);
                        End(lst, sb);
                    }else
                    {
                        End(lst, sb);
                        sb.Append(code[i]);
                    }
                }
                else if(isStringNow)
                {
                    sb.Append(code[i]);
                }
                else if (IsEmpty(code[i]))
                {

                }
                else if (IsSplit(code[i]))
                {
                    End(lst, sb);
                    if (code[i]==';')
                    {
                        sb.Append(code[i]);
                        End(lst, sb);
                    }
                }
                else if (IsNum(code[i]))
                {
                    if (IsOperator(sb.ToString()))
                    {
                        End(lst, sb);
                    }
                    sb.Append(code[i]);
                }
                else if (IsOperator(code[i]))
                {
                    if(!IsOperator(sb.ToString()+ code[i]))
                    {
                        End(lst, sb);
                    }
                    sb.Append(code[i]);
                }
                else
                {
                    if (IsNum(sb.ToString())||IsOperator(sb.ToString()))
                    {
                        End(lst, sb);
                        sb.Append(code[i]);
                    }
                    else
                    {
                        sb.Append(code[i]);
                    }
                }
            }
            End(lst, sb);


            return lst;


        }
        public void ManageDesc(List<LexicalNode> nodes)
        {
            for (int i = 0, icnt = nodes.Count; i < icnt; i++)
            {
                if (IsString(nodes[i]))
                {
                    nodes[i].desc = new Desc(nodes[i].rawCode.Substring(1, nodes[i].rawCode.Length - 2), CodeType.Str);
                }
                else if (IsNum(nodes[i].rawCode))
                {
                     nodes[i].desc= new Desc(nodes[i].rawCode, CodeType.Num);
                }
                else
                {
                    if (IsSplit(nodes[i].rawCode))
                    {
                        nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.Split);
                    }
                    else if (IsOperator(nodes[i].rawCode))
                    {
                        nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.Operator);
                    }
                    else if (IsReserved(nodes[i].rawCode))
                    {
                        nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.Reserved);
                    }
                    else if(IsFunc(nodes[i].rawCode))
                    {
                        nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.FuncName);
                    }
                    else
                    {
                        nodes[i].desc = new Desc(nodes[i].rawCode, CodeType.VarName);
                    }
                    
                }
            }
        }


        private bool End(List<LexicalNode> lst, StringBuilder sb)
        {
            if (sb.Length == 0)
            {
                return false;
            }
            else
            {
                lst.Add(new LexicalNode(sb.ToString()));
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
            foreach (var ch in str)
            {
                if (!IsNum(ch))
                    return false;
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
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsReserved(string str)
        {
            return BaseData.reserved.Contains(str);
        }
        public static bool IsFunc(string str)
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
