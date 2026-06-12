using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Debug;

namespace Z_Code
{
   


    public class ZLanguageAnalysis
    {
        public const bool DEBUG = true;

        /// <summary>
        /// 错误列表
        /// </summary>
        private List<CompileError> _errors;

        /// <summary>
        /// zCodeMap（每个 zCode 指令对应的原始代码位置）
        /// </summary>
        private List<int> _zCodeMap;

        public List<string> Execute(List<SyntaxNode> nodes, out List<int> zCodeMap, List<CompileError> errors = null)
        {
            _errors = errors ?? new List<CompileError>();
            _zCodeMap = new List<int>();
            List<string> cmds = new List<string>();
            foreach (var node in nodes)
            {
                try
                {
                    BuildZl(cmds, node);
                }
                catch (Exception ex)
                {
                    AddError(0, 0, "代码生成", $"生成代码时处理节点 '{node.desc.code}' 出错: {ex.Message}", ex);
                }
            }
            zCodeMap = _zCodeMap;
            if(DEBUG)
            {
                Z_Log.Log(cmds);
            }
            return cmds;
        }

        /// <summary>
        /// 添加错误
        /// </summary>
        private void AddError(int startIndex, int endIndex, string stage, string message, Exception ex = null)
        {
            _errors?.Add(new CompileError
            {
                StartIndex = startIndex,
                EndIndex = endIndex,
                LineNumber = 1,
                ColumnNumber = 1,
                Stage = stage,
                Message = message,
                Exception = ex
            });
        }

        /// <summary>
        /// 添加指令并记录原始代码位置
        /// </summary>
        private void AddCmd(List<string> cmds, string cmd, int codeIndex = -1)
        {
            cmds.Add(cmd);
            _zCodeMap.Add(codeIndex);
        }

        private void BuildZl(List<string> cmds, SyntaxNode node)
        {
            try
            {
                var codeIndex = node.desc.codeIndex;
                switch (node.desc.type)
                {
                    case CodeType.Num:
                        AddCmd(cmds, GetOpName(Op.PushNum), codeIndex);
                        AddCmd(cmds, node.desc.code, codeIndex);
                        break;
                    case CodeType.Str:
                        AddCmd(cmds, GetOpName(Op.PushStr), codeIndex);
                        AddCmd(cmds, node.desc.code, codeIndex);
                        break;
                    case CodeType.VarName:
                        AddCmd(cmds, GetOpName(Op.Get), codeIndex);
                        AddCmd(cmds, node.desc.code, codeIndex);
                        break;
                    case CodeType.Action:
                        for(int i = 0;i<node.subNodes.Count;i++)
                        {
                            try
                            {
                                BuildZl(cmds, node.subNodes[i]);
                            }
                            catch (Exception ex)
                            {
                                AddError(0, 0, "代码生成", $"处理Action子节点出错: {ex.Message}", ex);
                            }
                        }
                        break;
                    case CodeType.Reserved:
                        switch (node.desc.code)
                        {
                            case "Wait":
                                BuildZl(cmds, node.subNodes[0]);
                                AddCmd(cmds, GetOpName(Op.Wait), codeIndex);
                                break;
                            case "if":
                                BuildZl(cmds, node.subNodes[0]);
                                AddCmd(cmds, GetOpName(Op.IfFalseJump), codeIndex);
                                AddCmd(cmds, "-1", codeIndex);
                                int ifFalseJumpCmdId = cmds.Count - 1;
                                BuildZl(cmds, node.subNodes[1]);

                                AddCmd(cmds, GetOpName(Op.Jump), codeIndex);
                                AddCmd(cmds, "-1", codeIndex);
                                cmds[ifFalseJumpCmdId] = (cmds.Count).ToString();

                                int ifTrueJumpCmdId = cmds.Count - 1;
                                if (node.subNodes.Count > 2)
                                {
                                    BuildZl(cmds, node.subNodes[2]);
                                }
                                cmds[ifTrueJumpCmdId] = (cmds.Count).ToString();
                                break;
                            case "for":
                                BuildZl(cmds, node.subNodes[0]);

                                int forJumpId = cmds.Count;
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.IfFalseJump), codeIndex);
                                AddCmd(cmds, "-1", codeIndex);
                                int forFalseJumpCmdId = cmds.Count - 1;


                                BuildZl(cmds, node.subNodes[3]);
                                BuildZl(cmds, node.subNodes[2]);

                                AddCmd(cmds, GetOpName(Op.Jump), codeIndex);
                                AddCmd(cmds, forJumpId.ToString(), codeIndex);

                                cmds[forFalseJumpCmdId] = cmds.Count.ToString();
                                break;
                            case "Return":
                                BuildZl(cmds, node.subNodes[0]);
                                AddCmd(cmds, GetOpName(Op.Ret), codeIndex);
                                break;
                        }
                        break;
                    case CodeType.FuncName:
                        for(int i=0;i< node.subNodes.Count;i++)
                        {
                            try
                            {
                                BuildZl(cmds, node.subNodes[i]);
                            }
                            catch (Exception ex)
                            {
                                AddError(0, 0, "代码生成", $"处理函数参数出错: {ex.Message}", ex);
                            }
                        }
                        AddCmd(cmds, GetOpName(Op.PushNum), codeIndex);
                        AddCmd(cmds, node.subNodes.Count.ToString(), codeIndex);
                        AddCmd(cmds, GetOpName(Op.Call), codeIndex);
                        AddCmd(cmds, node.desc.code, codeIndex);
                        break;
                    case CodeType.Operator:
                        switch (node.desc.code)
                        {
                            case "+":
                                BuildZl(cmds, node.subNodes[0]);
                                if (node.subNodes.Count > 1)
                                { 
                                    BuildZl(cmds, node.subNodes[1]);
                                    AddCmd(cmds, GetOpName(Op.Plus), codeIndex);
                                }
                                else
                                {
                                    AddCmd(cmds, GetOpName(Op.Positive), codeIndex);
                                }
                                break;
                            case "-":
                                BuildZl(cmds, node.subNodes[0]);
                                if (node.subNodes.Count > 1)
                                {
                                    BuildZl(cmds, node.subNodes[1]);
                                    AddCmd(cmds, GetOpName(Op.Minus), codeIndex);
                                }
                                else
                                {
                                    AddCmd(cmds, GetOpName(Op.Negative), codeIndex);
                                }
                                break;
                            case "*":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.Mul), codeIndex);
                                break;
                            case "/":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.Div), codeIndex);
                                break;
                            case "=":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.Assign), codeIndex);
                                break;
                            case ".":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.Sub), codeIndex);
                                break;
                            case "==":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.Equal), codeIndex);
                                break;
                            case "!=":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.NotEqual), codeIndex);
                                break;
                            case ">":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.Greater), codeIndex);
                                break;
                            case "<":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.Less), codeIndex);
                                break;
                            case ">=":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.NotLess), codeIndex);
                                break;
                            case "<=":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.NotGreater), codeIndex);
                                break;
                            case "[":
                                BuildZl(cmds, node.subNodes[0]);
                                BuildZl(cmds, node.subNodes[1]);
                                AddCmd(cmds, GetOpName(Op.Take), codeIndex);
                                break;
                        }
                        break;

                }
            }
            catch (Exception ex)
            {
                AddError(0, 0, "代码生成", $"生成代码时出错: {ex.Message}", ex);
            }
        }
        public string GetOpName(Op op)
        {
            return ((int)op).ToString();
        }


    }
}
