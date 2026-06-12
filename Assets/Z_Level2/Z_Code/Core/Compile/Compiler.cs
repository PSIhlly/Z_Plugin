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

    /// <summary>
    /// 编译错误信息类
    /// </summary>
    public class CompileError
    {
        /// <summary>
        /// 错误在原始代码中的起始位置（字符索引）
        /// </summary>
        public int StartIndex;

        /// <summary>
        /// 错误在原始代码中的结束位置（字符索引）
        /// </summary>
        public int EndIndex;

        /// <summary>
        /// 错误所在行号（从1开始）
        /// </summary>
        public int LineNumber;

        /// <summary>
        /// 错误所在列号（从1开始）
        /// </summary>
        public int ColumnNumber;

        /// <summary>
        /// 错误描述
        /// </summary>
        public string Message;

        /// <summary>
        /// 错误阶段（词法分析/语法分析/代码生成）
        /// </summary>
        public string Stage;

        /// <summary>
        /// 原始异常（如果有）
        /// </summary>
        public Exception Exception;

        public override string ToString()
        {
            return $"[{Stage}] 行{LineNumber}列{ColumnNumber}: {Message}";
        }
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

        /// <summary>
        /// 原始代码（用于计算错误位置）
        /// </summary>
        private string _originalCode;

        /// <summary>
        /// 编译代码
        /// </summary>
        /// <param name="code">原始代码字符串</param>
        /// <param name="syntaxs">输出的语法树</param>
        /// <param name="paramCount">输出的参数数量</param>
        /// <param name="ret">输出的返回类型</param>
        /// <param name="zCodeMap">输出的 zCode 位置映射</param>
        /// <param name="errors">输出的错误列表</param>
        /// <returns>编译后的指令列表</returns>
        public List<string> Compile(string code, out List<SyntaxNode> syntaxs, out int paramCount, out string ret, out List<int> zCodeMap, out List<CompileError> errors)
        {
            errors = new List<CompileError>();
            _originalCode = code;
            syntaxs = new List<SyntaxNode>();
            paramCount = 0;
            ret = "void";
            zCodeMap = new List<int>();
            var zl = new List<string>();

            // 词法分析
            List<LexicalNode> lexicals = null;
            try
            {
                lexicals = lexicalAnalysis.Execute(code, errors);
            }
            catch (Exception ex)
            {
                errors.Add(CreateError(0, code.Length - 1, "词法分析", $"词法分析失败: {ex.Message}", ex));
                return zl;
            }

            // 语法分析
            try
            {
                syntaxs = syntaxAnalysis.Execute(lexicals, errors);
            }
            catch (Exception ex)
            {
                var errorPos = FindErrorPosition(lexicals);
                errors.Add(CreateError(errorPos.start, errorPos.end, "语法分析", $"语法分析失败: {ex.Message}", ex));
                return zl;
            }

            // Z语言分析（代码生成）
            try
            {
                zl = zLanguageAnalysis.Execute(syntaxs, out zCodeMap, errors);
            }
            catch (Exception ex)
            {
                var errorPos = FindErrorPosition(syntaxs);
                errors.Add(CreateError(errorPos.start, errorPos.end, "代码生成", $"代码生成失败: {ex.Message}", ex));
                return zl;
            }

            // 提取参数数量和返回类型
            try
            {
                var retRes = "void";
                var paramCountRes = 0;
                foreach (var node in syntaxs)
                {
                    try
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
                                if (o.subNodes.Count > 0)
                                {
                                    retRes = o.subNodes[0].desc.code;
                                }
                            }

                        }, node);
                    }
                    catch (Exception innerEx)
                    {
                        errors.Add(CreateError(0, _originalCode.Length - 1, "参数提取", $"提取参数信息失败: {innerEx.Message}", innerEx));
                    }
                }
                ret = retRes;
                paramCount = paramCountRes;
            }
            catch (Exception ex)
            {
                errors.Add(CreateError(0, _originalCode.Length - 1, "参数提取", $"提取参数信息失败: {ex.Message}", ex));
            }

            if (DEBUG)
            {
                Z_Log.Log(zl);
            }
            return zl;
        }

        /// <summary>
        /// 创建编译错误对象
        /// </summary>
        private CompileError CreateError(int startIndex, int endIndex, string stage, string message, Exception ex = null)
        {
            var (line, column) = GetLineAndColumn(startIndex);
            return new CompileError
            {
                StartIndex = startIndex,
                EndIndex = endIndex,
                LineNumber = line,
                ColumnNumber = column,
                Stage = stage,
                Message = message,
                Exception = ex
            };
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

        /// <summary>
        /// 从词法节点列表中查找可能的错误位置
        /// </summary>
        private (int start, int end) FindErrorPosition(List<LexicalNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return (0, 0);
            }
            // 返回最后一个节点的位置作为估计
            return (0, _originalCode.Length - 1);
        }

        /// <summary>
        /// 从语法节点列表中查找可能的错误位置
        /// </summary>
        private (int start, int end) FindErrorPosition(List<SyntaxNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return (0, 0);
            }
            return (0, _originalCode.Length - 1);
        }

        public void DfsNode(Action<SyntaxNode> manage, SyntaxNode node)
        {
            manage(node);
            foreach (var sub in node.subNodes)
            {
                DfsNode(manage, sub);
            }

        }

    }
}
