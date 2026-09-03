using System;
using System.Collections.Generic;
using Z_Debug;

namespace Z_Code
{
    public class ZLanguageAnalysis
    {
        public static bool DEBUG = false;

        private sealed class LoopContext
        {
            public readonly List<int> breakOperands = new List<int>();
            public readonly List<int> continueOperands = new List<int>();
            public int continueTarget = -1;
        }

        private List<CompileError> _errors;
        private List<int> _zCodeMap;
        private readonly Stack<LoopContext> _loops = new Stack<LoopContext>();
        private int _buildDepth;

        public List<string> Execute(List<SyntaxNode> nodes, out List<int> zCodeMap,
            List<CompileError> errors = null)
        {
            _errors = errors ?? new List<CompileError>();
            _zCodeMap = new List<int>();
            _loops.Clear();
            _buildDepth = 0;

            var commands = new List<string>();
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    try
                    {
                        BuildStatement(commands, node);
                    }
                    catch (Exception ex)
                    {
                        AddError(node, $"生成节点 '{node?.desc?.code}' 失败: {ex.Message}", ex);
                    }
                }
            }

            zCodeMap = _zCodeMap;
            if (DEBUG || Compiler.DEBUG)
            {
                Z_Log.Log(commands);
            }
            return commands;
        }

        private void BuildZl(List<string> commands, SyntaxNode node)
        {
            if (node?.desc == null)
            {
                throw new InvalidOperationException("语法树包含空节点");
            }
            if (_buildDepth >= SyntaxAnalysis.MaxNestingDepth)
            {
                throw new InvalidOperationException(
                    $"语法树嵌套超过最大限制 {SyntaxAnalysis.MaxNestingDepth}");
            }

            _buildDepth++;
            try
            {
                int codeIndex = node.desc.codeIndex;
                switch (node.desc.type)
                {
                    case CodeType.Num:
                        PushNumber(commands, node.desc.code, codeIndex);
                        return;
                    case CodeType.Str:
                        AddCmd(commands, Op.PushStr, codeIndex);
                        AddCmd(commands, node.desc.code, codeIndex);
                        return;
                    case CodeType.VarName:
                        AddCmd(commands, Op.Get, codeIndex);
                        AddCmd(commands, node.desc.code, codeIndex);
                        return;
                    case CodeType.Action:
                        foreach (var child in node.subNodes)
                        {
                            BuildStatement(commands, child);
                        }
                        return;
                    case CodeType.Reserved:
                        BuildReserved(commands, node);
                        return;
                    case CodeType.FuncName:
                        BuildCall(commands, node, false);
                        return;
                    case CodeType.Operator:
                        BuildOperator(commands, node);
                        return;
                    default:
                        throw new InvalidOperationException($"不支持的节点类型 {node.desc.type}");
                }
            }
            finally
            {
                _buildDepth--;
            }
        }

        private void BuildStatement(List<string> commands, SyntaxNode node)
        {
            if (node?.desc?.type == CodeType.FuncName)
            {
                BuildCall(commands, node, true);
                return;
            }

            BuildZl(commands, node);
            if (ProducesValue(node))
            {
                AddCmd(commands, Op.Discard, node.desc.codeIndex);
            }
        }

        private void BuildCall(List<string> commands, SyntaxNode node, bool discardReturn)
        {
            foreach (var argument in node.subNodes)
            {
                if (SyntaxAnalysis.IsEmptyArgumentNode(argument))
                {
                    AddCmd(commands, Op.PushStr, argument.desc.codeIndex);
                    AddCmd(commands, string.Empty, argument.desc.codeIndex);
                }
                else
                {
                    BuildZl(commands, argument);
                }
            }
            PushNumber(commands, node.subNodes.Count.ToString(), node.desc.codeIndex);
            AddCmd(commands, discardReturn ? Op.CallDiscard : Op.Call, node.desc.codeIndex);
            AddCmd(commands, node.desc.code, node.desc.codeIndex);
        }

        private static bool ProducesValue(SyntaxNode node)
        {
            if (node?.desc == null)
            {
                return false;
            }

            switch (node.desc.type)
            {
                case CodeType.Num:
                case CodeType.Str:
                case CodeType.VarName:
                    return true;
                case CodeType.Operator:
                    return node.desc.code != "=";
                default:
                    return false;
            }
        }

        private void BuildReserved(List<string> commands, SyntaxNode node)
        {
            int codeIndex = node.desc.codeIndex;
            switch (node.desc.code)
            {
                case "Wait":
                    if (node.subNodes.Count == 0)
                    {
                        PushNumber(commands, "0", codeIndex);
                    }
                    else
                    {
                        BuildZl(commands, node.subNodes[0]);
                    }
                    AddCmd(commands, Op.Wait, codeIndex);
                    return;
                case "Return":
                    if (node.subNodes.Count == 0)
                    {
                        PushNumber(commands, "0", codeIndex);
                    }
                    else
                    {
                        BuildZl(commands, node.subNodes[0]);
                    }
                    AddCmd(commands, Op.Ret, codeIndex);
                    return;
                case "if":
                    BuildIf(commands, node);
                    return;
                case "for":
                    BuildFor(commands, node);
                    return;
                case "while":
                    BuildWhile(commands, node);
                    return;
                case "break":
                    EmitLoopJump(commands, node, true);
                    return;
                case "continue":
                    EmitLoopJump(commands, node, false);
                    return;
                default:
                    throw new InvalidOperationException($"未知保留语句 {node.desc.code}");
            }
        }

        private void BuildIf(List<string> commands, SyntaxNode node)
        {
            RequireChildren(node, 2);
            BuildZl(commands, node.subNodes[0]);
            int falseOperand = EmitJumpPlaceholder(commands, Op.IfFalseJump, node.desc.codeIndex);
            BuildZl(commands, node.subNodes[1]);

            if (node.subNodes.Count > 2)
            {
                int endOperand = EmitJumpPlaceholder(commands, Op.Jump, node.desc.codeIndex);
                Patch(commands, falseOperand, commands.Count);
                BuildZl(commands, node.subNodes[2]);
                Patch(commands, endOperand, commands.Count);
            }
            else
            {
                Patch(commands, falseOperand, commands.Count);
            }
        }

        private void BuildWhile(List<string> commands, SyntaxNode node)
        {
            RequireChildren(node, 2);
            int conditionTarget = commands.Count;
            BuildZl(commands, node.subNodes[0]);
            int falseOperand = EmitJumpPlaceholder(commands, Op.IfFalseJump, node.desc.codeIndex);

            var loop = new LoopContext { continueTarget = conditionTarget };
            _loops.Push(loop);
            BuildZl(commands, node.subNodes[1]);
            _loops.Pop();

            PatchAll(commands, loop.continueOperands, conditionTarget);
            EmitJump(commands, conditionTarget, node.desc.codeIndex);
            int endTarget = commands.Count;
            Patch(commands, falseOperand, endTarget);
            PatchAll(commands, loop.breakOperands, endTarget);
        }

        private void BuildFor(List<string> commands, SyntaxNode node)
        {
            RequireChildren(node, 4);
            if (!SyntaxAnalysis.IsEmptyNode(node.subNodes[0]))
            {
                BuildStatement(commands, node.subNodes[0]);
            }

            int conditionTarget = commands.Count;
            int falseOperand = -1;
            if (!SyntaxAnalysis.IsEmptyNode(node.subNodes[1]))
            {
                BuildZl(commands, node.subNodes[1]);
                falseOperand = EmitJumpPlaceholder(commands, Op.IfFalseJump, node.desc.codeIndex);
            }

            var loop = new LoopContext();
            _loops.Push(loop);
            BuildZl(commands, node.subNodes[3]);
            _loops.Pop();

            loop.continueTarget = commands.Count;
            PatchAll(commands, loop.continueOperands, loop.continueTarget);
            if (!SyntaxAnalysis.IsEmptyNode(node.subNodes[2]))
            {
                BuildStatement(commands, node.subNodes[2]);
            }
            EmitJump(commands, conditionTarget, node.desc.codeIndex);

            int endTarget = commands.Count;
            if (falseOperand >= 0)
            {
                Patch(commands, falseOperand, endTarget);
            }
            PatchAll(commands, loop.breakOperands, endTarget);
        }

        private void EmitLoopJump(List<string> commands, SyntaxNode node, bool isBreak)
        {
            if (_loops.Count == 0)
            {
                AddError(node, $"{node.desc.code} 只能出现在循环内部");
                return;
            }

            int operand = EmitJumpPlaceholder(commands, Op.Jump, node.desc.codeIndex);
            if (isBreak)
            {
                _loops.Peek().breakOperands.Add(operand);
            }
            else
            {
                _loops.Peek().continueOperands.Add(operand);
            }
        }

        private void BuildOperator(List<string> commands, SyntaxNode node)
        {
            switch (node.desc.code)
            {
                case "&&":
                    BuildLogicalAnd(commands, node);
                    return;
                case "||":
                    BuildLogicalOr(commands, node);
                    return;
                case "!":
                    RequireChildren(node, 1);
                    BuildZl(commands, node.subNodes[0]);
                    AddCmd(commands, Op.Not, node.desc.codeIndex);
                    return;
                case "+":
                    RequireChildren(node, 1);
                    BuildZl(commands, node.subNodes[0]);
                    if (node.subNodes.Count == 1)
                    {
                        AddCmd(commands, Op.Positive, node.desc.codeIndex);
                    }
                    else
                    {
                        BuildZl(commands, node.subNodes[1]);
                        AddCmd(commands, Op.Plus, node.desc.codeIndex);
                    }
                    return;
                case "-":
                    RequireChildren(node, 1);
                    BuildZl(commands, node.subNodes[0]);
                    if (node.subNodes.Count == 1)
                    {
                        AddCmd(commands, Op.Negative, node.desc.codeIndex);
                    }
                    else
                    {
                        BuildZl(commands, node.subNodes[1]);
                        AddCmd(commands, Op.Minus, node.desc.codeIndex);
                    }
                    return;
            }

            RequireChildren(node, 2);
            // Historical binary AST order is [right, left], and the stack VM expects right then left.
            BuildZl(commands, node.subNodes[0]);
            BuildZl(commands, node.subNodes[1]);
            switch (node.desc.code)
            {
                case "*": AddCmd(commands, Op.Mul, node.desc.codeIndex); break;
                case "/": AddCmd(commands, Op.Div, node.desc.codeIndex); break;
                case "%": AddCmd(commands, Op.Mod, node.desc.codeIndex); break;
                case "=": AddCmd(commands, Op.Assign, node.desc.codeIndex); break;
                case ".": AddCmd(commands, Op.Sub, node.desc.codeIndex); break;
                case "==": AddCmd(commands, Op.Equal, node.desc.codeIndex); break;
                case "!=": AddCmd(commands, Op.NotEqual, node.desc.codeIndex); break;
                case ">": AddCmd(commands, Op.Greater, node.desc.codeIndex); break;
                case "<": AddCmd(commands, Op.Less, node.desc.codeIndex); break;
                case ">=": AddCmd(commands, Op.NotLess, node.desc.codeIndex); break;
                case "<=": AddCmd(commands, Op.NotGreater, node.desc.codeIndex); break;
                case "[": AddCmd(commands, Op.Take, node.desc.codeIndex); break;
                default: throw new InvalidOperationException($"未知运算符 {node.desc.code}");
            }
        }

        private void BuildLogicalAnd(List<string> commands, SyntaxNode node)
        {
            RequireChildren(node, 2);
            // Evaluate left first for short-circuiting. Binary children are [right, left].
            BuildZl(commands, node.subNodes[1]);
            int leftFalse = EmitJumpPlaceholder(commands, Op.IfFalseJump, node.desc.codeIndex);
            BuildZl(commands, node.subNodes[0]);
            int rightFalse = EmitJumpPlaceholder(commands, Op.IfFalseJump, node.desc.codeIndex);
            PushNumber(commands, "1", node.desc.codeIndex);
            int endOperand = EmitJumpPlaceholder(commands, Op.Jump, node.desc.codeIndex);
            int falseTarget = commands.Count;
            PushNumber(commands, "0", node.desc.codeIndex);
            int endTarget = commands.Count;
            Patch(commands, leftFalse, falseTarget);
            Patch(commands, rightFalse, falseTarget);
            Patch(commands, endOperand, endTarget);
        }

        private void BuildLogicalOr(List<string> commands, SyntaxNode node)
        {
            RequireChildren(node, 2);
            BuildZl(commands, node.subNodes[1]);
            int evaluateRight = EmitJumpPlaceholder(commands, Op.IfFalseJump, node.desc.codeIndex);
            PushNumber(commands, "1", node.desc.codeIndex);
            int leftTrueEnd = EmitJumpPlaceholder(commands, Op.Jump, node.desc.codeIndex);

            Patch(commands, evaluateRight, commands.Count);
            BuildZl(commands, node.subNodes[0]);
            int rightFalse = EmitJumpPlaceholder(commands, Op.IfFalseJump, node.desc.codeIndex);
            PushNumber(commands, "1", node.desc.codeIndex);
            int rightTrueEnd = EmitJumpPlaceholder(commands, Op.Jump, node.desc.codeIndex);

            int falseTarget = commands.Count;
            PushNumber(commands, "0", node.desc.codeIndex);
            int endTarget = commands.Count;
            Patch(commands, rightFalse, falseTarget);
            Patch(commands, leftTrueEnd, endTarget);
            Patch(commands, rightTrueEnd, endTarget);
        }

        private static void RequireChildren(SyntaxNode node, int minimum)
        {
            if (node.subNodes.Count < minimum)
            {
                throw new InvalidOperationException($"节点 {node.desc.code} 至少需要 {minimum} 个子节点");
            }
        }

        private int EmitJumpPlaceholder(List<string> commands, Op op, int codeIndex)
        {
            AddCmd(commands, op, codeIndex);
            AddCmd(commands, "-1", codeIndex);
            return commands.Count - 1;
        }

        private void EmitJump(List<string> commands, int target, int codeIndex)
        {
            AddCmd(commands, Op.Jump, codeIndex);
            AddCmd(commands, target.ToString(), codeIndex);
        }

        private static void Patch(List<string> commands, int operandIndex, int target)
        {
            commands[operandIndex] = target.ToString();
        }

        private static void PatchAll(List<string> commands, List<int> operands, int target)
        {
            foreach (int operand in operands)
            {
                Patch(commands, operand, target);
            }
        }

        private void PushNumber(List<string> commands, string number, int codeIndex)
        {
            AddCmd(commands, Op.PushNum, codeIndex);
            AddCmd(commands, number, codeIndex);
        }

        private void AddCmd(List<string> commands, Op op, int codeIndex)
        {
            AddCmd(commands, ((int)op).ToString(), codeIndex);
        }

        private void AddCmd(List<string> commands, string command, int codeIndex)
        {
            commands.Add(command);
            _zCodeMap.Add(codeIndex);
        }

        private void AddError(SyntaxNode node, string message, Exception exception = null)
        {
            int index = node?.desc?.codeIndex ?? 0;
            _errors.Add(new CompileError
            {
                StartIndex = index,
                EndIndex = index,
                LineNumber = 1,
                ColumnNumber = 1,
                Stage = "代码生成",
                Message = message,
                Exception = exception
            });
        }
    }
}
