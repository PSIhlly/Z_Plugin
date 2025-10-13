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
        public List<string> Execute(List<SyntaxNode> nodes)
        {
            List<string> cmds = new List<string>();
            foreach (var node in nodes)
            {
                BuildZl(cmds, node);
            }
            if(DEBUG)
            {
                Z_Log.Log(cmds);
            }
            return cmds;
        }
        private void BuildZl(List<string> cmds, SyntaxNode node)
        {
            switch (node.desc.type)
            {
                case CodeType.Num:
                    cmds.Add(GetOpName(Op.PushNum));
                    cmds.Add(node.desc.code);
                    break;
                case CodeType.Str:
                    cmds.Add(GetOpName(Op.PushStr));
                    cmds.Add(node.desc.code);
                    break;
                case CodeType.VarName:
                    cmds.Add(GetOpName(Op.Get));
                    cmds.Add(node.desc.code);
                    break;
                case CodeType.Action:
                    for(int i = 0;i<node.subNodes.Count;i++)
                    {
                        BuildZl(cmds, node.subNodes[i]);
                    }
                    break;
                case CodeType.Reserved:
                    switch (node.desc.code)
                    {
                        case "if":
                            BuildZl(cmds, node.subNodes[0]);
                            cmds.Add(GetOpName(Op.IfFalseJump));
                            cmds.Add("-1");
                            int ifFalseJumpCmdId = cmds.Count - 1;
                            BuildZl(cmds, node.subNodes[1]);

                            cmds.Add(GetOpName(Op.Jump));
                            cmds.Add("-1");
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
                            cmds.Add(GetOpName(Op.IfFalseJump));
                            cmds.Add("-1");
                            int forFalseJumpCmdId = cmds.Count - 1;


                            BuildZl(cmds, node.subNodes[3]);
                            BuildZl(cmds, node.subNodes[2]);

                            cmds.Add(GetOpName(Op.Jump));
                            cmds.Add(forJumpId.ToString());

                            cmds[forFalseJumpCmdId] = cmds.Count.ToString();
                            break;
                    }
                    break;
                case CodeType.FuncName:
                    for(int i=0;i< node.subNodes.Count;i++)
                    {
                        BuildZl(cmds, node.subNodes[i]);
                    }
                    cmds.Add(GetOpName(Op.Call));
                    cmds.Add(node.desc.code);
                    break;
                case CodeType.Operator:
                    switch (node.desc.code)
                    {
                        case "+":
                            BuildZl(cmds, node.subNodes[0]);
                            BuildZl(cmds, node.subNodes[1]);
                            cmds.Add(GetOpName(Op.Plus));
                            break;
                        case "-":
                            BuildZl(cmds, node.subNodes[0]);
                            BuildZl(cmds, node.subNodes[1]);
                            cmds.Add(GetOpName(Op.Minus));
                            break;
                        case "*":
                            BuildZl(cmds, node.subNodes[0]);
                            BuildZl(cmds, node.subNodes[1]);
                            cmds.Add(GetOpName(Op.Mul));
                            break;
                        case "/":
                            BuildZl(cmds, node.subNodes[0]);
                            BuildZl(cmds, node.subNodes[1]);
                            cmds.Add(GetOpName(Op.Div));
                            break;
                        case "=":
                            BuildZl(cmds, node.subNodes[0]);
                            BuildZl(cmds, node.subNodes[1]);
                            cmds.Add(GetOpName(Op.Assign));
                            break;
                        case ".":
                            BuildZl(cmds, node.subNodes[0]);
                            BuildZl(cmds, node.subNodes[1]);
                            cmds.Add(GetOpName(Op.Sub));
                            break;
                        case "==":
                            BuildZl(cmds, node.subNodes[0]);
                            BuildZl(cmds, node.subNodes[1]);
                            cmds.Add(GetOpName(Op.Equal));
                            break;
                        case "!=":
                            BuildZl(cmds, node.subNodes[0]);
                            BuildZl(cmds, node.subNodes[1]);
                            cmds.Add(GetOpName(Op.NotEqual));
                            break;
                    }
                    break;

            }
        }
        public string GetOpName(Op op)
        {
            return ((int)op).ToString();
        }


    }
}
