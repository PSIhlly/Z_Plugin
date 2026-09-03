//#define INTERPRETER_DEBUG
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Z_Code.Form;
using Z_Debug;
using static Z_Code.Form.InterpretDataForm;

namespace Z_Code
{
    public enum Op
    {
        PushNum,
        PushStr,
        Call,
        Plus,
        Positive,
        Negative,
        Minus,
        Mul,
        Div,
        Assign,
        Equal,
        NotEqual,
        Take,
        Sub,
        Get,
        IfFalseJump,
        Jump,
        Wait,
        Greater,
        Less,
        NotGreater,
        NotLess,
        Ret,
        Mod,
        Not,
        CallDiscard,
        Discard
    }

    internal sealed class InterpretBudget
    {
        private const int MaxSubProgramDepth = 64;
        private int subProgramDepth;

        public InterpretBudget(int instructionCount)
        {
            RemainingInstructions = Math.Max(0, instructionCount);
        }

        public int RemainingInstructions { get; private set; }

        public bool TryConsumeInstruction()
        {
            if (RemainingInstructions <= 0)
            {
                return false;
            }

            RemainingInstructions--;
            return true;
        }

        public bool TryEnterSubProgram()
        {
            if (subProgramDepth >= MaxSubProgramDepth)
            {
                return false;
            }

            subProgramDepth++;
            return true;
        }

        public void ExitSubProgram()
        {
            if (subProgramDepth > 0)
            {
                subProgramDepth--;
            }
        }
    }

    namespace Form
    {
        public static partial class InterpretDataForm
        {
            public class RetInfo
            {
                public bool complete;
                public BoxDataForm.Data ret = CodeHelper.CreateBox();
                public List<InterpretError> errors = new List<InterpretError>();
            }

            public partial class Data
            {
                public int debugId;
                protected Interpreter _interpreter;

                public virtual RetInfo Interpret()
                {
                    return Interpret(new InterpretBudget(Interpreter.MaxInstructionsPerExecution));
                }

                internal RetInfo Interpret(InterpretBudget budget)
                {
                    if (_interpreter == null)
                    {
                        _interpreter = new Interpreter(this);
                    }

                    return _interpreter.Interpret(budget);
                }

                public void Reset()
                {
                    _interpreter?.Reset();
                    p = 0;
                    stack?.Clear();
                    top = -1;
                    heap?.Clear();
                    heapTemp?.Clear();
                    subInterpret = null;
                }
            }

            /// <summary>
            /// 解释器运行时错误
            /// </summary>
            public class InterpretError
            {
                /// <summary>
                /// 当前指令地址（PC）
                /// </summary>
                public int Pc;

                /// <summary>
                /// 当前操作码
                /// </summary>
                public Op OpCode;

                /// <summary>
                /// 错误描述
                /// </summary>
                public string Message;

                /// <summary>
                /// 原始异常
                /// </summary>
                public Exception Exception;

                /// <summary>
                /// 原始代码中的行号（从1开始）
                /// </summary>
                public int LineNumber;

                /// <summary>
                /// 原始代码中的列号（从1开始）
                /// </summary>
                public int ColumnNumber;

                /// <summary>
                /// 原始代码中对应行的内容
                /// </summary>
                public string SourceLine;

                /// <summary>
                /// 程序名称
                /// </summary>
                public string ProgramName;

                public override string ToString()
                {
                    var programInfo = !string.IsNullOrEmpty(ProgramName) ? $"[{ProgramName}] " : "";
                    var location = LineNumber > 0 ? $"行{LineNumber}列{ColumnNumber}" : $"PC={Pc}";
                    var sourceInfo = !string.IsNullOrEmpty(SourceLine) ? $"\n  源代码: {SourceLine}" : "";
                    return $"{programInfo}{location}, Op={OpCode}: {Message}{sourceInfo}";
                }
            }
        }
    }

    public class InterpretAsyncTask
    {
        public readonly Interpreter interpreter;
        public BoxDataForm.Data[] res;
        public string error;

        public InterpretAsyncTask(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        private bool isRuning;
        private bool isComplete;

        public bool IsRuning()
        {
            return isRuning;
        }

        public bool IsComplete()
        {
            return isComplete;
        }

        public void Run()
        {
            isComplete = false;
            isRuning = true;
            error = null;
            res = null;
        }

        public void Complete()
        {
            isComplete = true;
            isRuning = false;
        }

        public void Reset()
        {
            isComplete = false;
            isRuning = false;
            error = null;
            res = null;
        }
    }

    public class Interpreter
    {
        /// <summary>
        /// 单次顶层 Interpret 调用（包含同步子程序）最多执行的操作码数量。
        /// 耗尽后保留 PC/栈状态并在下一帧续跑，不视为运行时错误。
        /// </summary>
        public const int MaxInstructionsPerExecution = 4096;

        public InterpretDataForm.Data data;
        private Op? opCode;
        private readonly InterpretAsyncTask asyncTask;

        private List<string> cachedZCode;
        private int cachedZCodeCount = -1;
        private Op[] opcodeCache;
        private bool[] opcodeCacheValid;
        private float[] numberCache;
        private bool[] numberCacheValid;
        private int[] integerCache;
        private bool[] integerCacheValid;

        /// <summary>
        /// 解释执行过程中收集的错误信息
        /// </summary>
        public readonly List<InterpretError> errors = new List<InterpretError>();

        public Interpreter(InterpretDataForm.Data interpret)
        {
            data = interpret;
            asyncTask = new InterpretAsyncTask(this);
        }

        public RetInfo Interpret()
        {
            return Interpret(new InterpretBudget(MaxInstructionsPerExecution));
        }

        internal RetInfo Interpret(InterpretBudget budget)
        {
            errors.Clear();
            opCode = null;

            if (!TryPrepare(out var zCode))
            {
                return MakeRetInfo(true);
            }

            budget ??= new InterpretBudget(MaxInstructionsPerExecution);
            int count = zCode.Count;

#if INTERPRETER_DEBUG
            Z_Log.Log(data.debugId + "[Start]" + data.program.code);
#endif

            while (data.p < count)
            {
                if (!budget.TryConsumeInstruction())
                {
                    return MakeRetInfo(false);
                }

                int instructionPc = data.p;
                if (!TryReadOpcode(zCode, instructionPc, out var parsedOp, out var parseError))
                {
                    AddError(instructionPc, parseError);
                    return MakeRetInfo(true);
                }

                opCode = parsedOp;

#if INTERPRETER_DEBUG
                Z_Log.Log(data.p + ":" + parsedOp);
                Z_Log.Log("{Current Stacks:}");
                for (int i = 0; i < data.stack.Count; i++)
                {
                    Z_Log.Log("{" + i + " val:" + data.stack[i].valName + " num:" + data.stack[i].num + " str:" + data.stack[i].str + " dic:" + data.stack[i].dic.Count + "}");
                }
#endif

                try
                {
                    switch (parsedOp)
                    {
                        case Op.PushNum:
                        {
                            int operandIndex = GetOperandIndex(zCode, instructionPc, parsedOp);
                            if (!TryReadNumber(zCode, operandIndex, out var number, out parseError))
                            {
                                throw new FormatException(parseError);
                            }

                            Push(CodeHelper.CreateBoxByNum(number));
                            data.p += 2;
                            break;
                        }
                        case Op.PushStr:
                        {
                            int operandIndex = GetOperandIndex(zCode, instructionPc, parsedOp);
                            Push(CodeHelper.CreateBoxByStr(zCode[operandIndex]));
                            data.p += 2;
                            break;
                        }
                        case Op.Get:
                        {
                            int operandIndex = GetOperandIndex(zCode, instructionPc, parsedOp);
                            Push(CodeHelper.CreateBoxByVal(zCode[operandIndex]));
                            data.p += 2;
                            break;
                        }
                        case Op.Call:
                        case Op.CallDiscard:
                        {
                            int operandIndex = GetOperandIndex(zCode, instructionPc, parsedOp);
                            var callResult = ExecuteCall(zCode[operandIndex], budget, instructionPc,
                                parsedOp == Op.CallDiscard);
                            if (callResult != null)
                            {
                                return callResult;
                            }

                            break;
                        }
                        case Op.Equal:
                        {
                            var left = GetBox(Pop());
                            var right = GetBox(Pop());
                            if (left.str == null && right.str == null)
                            {
                                Push(CodeHelper.CreateBoxByNum(GetNum(left) == GetNum(right) ? 1 : 0));
                            }
                            else
                            {
                                Push(CodeHelper.CreateBoxByNum(GetStr(left) == GetStr(right) ? 1 : 0));
                            }

                            data.p++;
                            break;
                        }
                        case Op.Greater:
                        {
                            float left = GetNum(Pop());
                            float right = GetNum(Pop());
                            Push(CodeHelper.CreateBoxByNum(left > right ? 1 : 0));
                            data.p++;
                            break;
                        }
                        case Op.Less:
                        {
                            float left = GetNum(Pop());
                            float right = GetNum(Pop());
                            Push(CodeHelper.CreateBoxByNum(left < right ? 1 : 0));
                            data.p++;
                            break;
                        }
                        case Op.NotGreater:
                        {
                            float left = GetNum(Pop());
                            float right = GetNum(Pop());
                            Push(CodeHelper.CreateBoxByNum(left <= right ? 1 : 0));
                            data.p++;
                            break;
                        }
                        case Op.NotLess:
                        {
                            float left = GetNum(Pop());
                            float right = GetNum(Pop());
                            Push(CodeHelper.CreateBoxByNum(left >= right ? 1 : 0));
                            data.p++;
                            break;
                        }
                        case Op.NotEqual:
                        {
                            var left = GetBox(Pop());
                            var right = GetBox(Pop());
                            if (left.str == null && right.str == null)
                            {
                                Push(CodeHelper.CreateBoxByNum(GetNum(left) != GetNum(right) ? 1 : 0));
                            }
                            else
                            {
                                Push(CodeHelper.CreateBoxByNum(GetStr(left) != GetStr(right) ? 1 : 0));
                            }

                            data.p++;
                            break;
                        }
                        case Op.Take:
                        {
                            var owner = Pop();
                            string key = GetStr(Pop());
                            var reference = CodeHelper.CreateBoxByVal(owner.valName);
                            reference.str = key;
                            Push(reference);
                            data.p++;
                            break;
                        }
                        case Op.Assign:
                        {
                            var target = Pop();
                            if (string.IsNullOrEmpty(target.valName))
                            {
                                throw new InvalidOperationException("赋值目标不是变量或成员");
                            }

                            var value = GetBox(Pop()).DeepCopy();
                            if (target.str != null)
                            {
                                GetBox(target);
                                data.heap[target.valName].dic[target.str] = value;
                            }
                            else
                            {
                                data.heap[target.valName] = value;
                            }

                            data.p++;
                            break;
                        }
                        case Op.Plus:
                        {
                            var left = GetBox(Pop());
                            var right = GetBox(Pop());
                            if (HasDictionary(left) && HasDictionary(right))
                            {
                                var result = CodeHelper.CreateBox();
                                foreach (var pair in left.dic)
                                {
                                    if (right.dic.TryGetValue(pair.Key, out var rightValue))
                                    {
                                        result.dic[pair.Key] = ValuePlus(pair.Value, rightValue);
                                    }
                                }

                                Push(result);
                            }
                            else
                            {
                                Push(ValuePlus(left, right));
                            }

                            data.p++;
                            break;
                        }
                        case Op.Positive:
                            Push(CodeHelper.CreateBoxByNum(GetNum(Pop())));
                            data.p++;
                            break;
                        case Op.Minus:
                        {
                            var left = GetBox(Pop());
                            var right = GetBox(Pop());
                            if (HasDictionary(left) && HasDictionary(right))
                            {
                                var result = CodeHelper.CreateBox();
                                foreach (var pair in left.dic)
                                {
                                    if (right.dic.TryGetValue(pair.Key, out var rightValue))
                                    {
                                        result.dic[pair.Key] = ValueMinus(pair.Value, rightValue);
                                    }
                                }

                                Push(result);
                            }
                            else
                            {
                                Push(ValueMinus(left, right));
                            }

                            data.p++;
                            break;
                        }
                        case Op.Negative:
                            Push(CodeHelper.CreateBoxByNum(-GetNum(Pop())));
                            data.p++;
                            break;
                        case Op.Mul:
                        {
                            float left = GetNum(Pop());
                            float right = GetNum(Pop());
                            Push(CodeHelper.CreateBoxByNum(left * right));
                            data.p++;
                            break;
                        }
                        case Op.Div:
                        {
                            float left = GetNum(Pop());
                            float right = GetNum(Pop());
                            if (right == 0f)
                            {
                                throw new DivideByZeroException("除数不能为 0");
                            }

                            Push(CodeHelper.CreateBoxByNum(left / right));
                            data.p++;
                            break;
                        }
                        case Op.Mod:
                        {
                            float left = GetNum(Pop());
                            float right = GetNum(Pop());
                            if (right == 0f)
                            {
                                throw new DivideByZeroException("取模除数不能为 0");
                            }

                            Push(CodeHelper.CreateBoxByNum(left % right));
                            data.p++;
                            break;
                        }
                        case Op.Not:
                            Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) == 0f ? 1 : 0));
                            data.p++;
                            break;
                        case Op.Discard:
                            Pop();
                            data.p++;
                            break;
                        case Op.Jump:
                        {
                            int operandIndex = GetOperandIndex(zCode, instructionPc, parsedOp);
                            data.p = ReadJumpTarget(zCode, operandIndex);
                            break;
                        }
                        case Op.IfFalseJump:
                        {
                            int operandIndex = GetOperandIndex(zCode, instructionPc, parsedOp);
                            int target = ReadJumpTarget(zCode, operandIndex);
                            data.p = GetNum(Pop()) == 0f ? target : data.p + 2;
                            break;
                        }
                        case Op.Sub:
                        {
                            var owner = Pop();
                            var member = Pop();
                            if (string.IsNullOrEmpty(owner.valName) || string.IsNullOrEmpty(member.valName))
                            {
                                throw new InvalidOperationException("成员访问必须使用变量名");
                            }

                            var reference = CodeHelper.CreateBoxByVal(owner.valName);
                            reference.str = member.valName;
                            Push(reference);
                            data.p++;
                            break;
                        }
                        case Op.Wait:
                        {
                            var waitValue = Pop();
                            if (waitValue.valName != null && waitValue.num == 0f)
                            {
                                waitValue.num = GetBox(waitValue).num;
                            }

                            waitValue.num -= Time.deltaTime;
                            if (waitValue.num > 0f)
                            {
                                Push(waitValue);
                                return MakeRetInfo(false);
                            }

                            data.p++;
                            break;
                        }
                        case Op.Ret:
                        {
                            var result = GetBox(Pop());
                            data.p++;
                            return MakeRetInfo(true, result);
                        }
                        default:
                            throw new InvalidOperationException($"不支持的操作码: {parsedOp}");
                    }
                }
                catch (Exception ex)
                {
                    AddError(instructionPc, $"执行 {parsedOp} 指令失败: {ex.Message}", ex);
                    return MakeRetInfo(true);
                }
                finally
                {
                    opCode = null;
                }
            }

            return MakeRetInfo(true);
        }

        private RetInfo ExecuteCall(string funcName, InterpretBudget budget, int instructionPc,
            bool discardReturn)
        {
#if INTERPRETER_DEBUG
            Z_Log.Log(" invoke" + funcName);
#endif
            if (string.IsNullOrEmpty(funcName))
            {
                throw new InvalidOperationException("函数名为空");
            }

            int parameterCount = GetParameterCount();
            if (BaseData.cmdDic.TryGetValue(funcName, out var cmdTemplate))
            {
                var cmd = cmdTemplate.GetNew();
                var form = cmd.GetForm();
                int expectedParameterCount = form.prmNames == null ? 0 : form.prmNames.Count;
                if (parameterCount > expectedParameterCount)
                {
                    throw new InvalidOperationException($"命令 {funcName} 最多接受 {expectedParameterCount} 个参数，实际为 {parameterCount} 个");
                }

                var parameters = new BoxDataForm.Data[expectedParameterCount];
                int missingParameterCount = expectedParameterCount - parameterCount;
                for (int i = 0; i < missingParameterCount; i++)
                {
                    // CmdBase reverses the array immediately before execution. Prefixing here therefore
                    // appends omitted arguments after the caller-supplied arguments in source order.
                    parameters[i] = CodeHelper.CreateBoxByStr(string.Empty);
                }
                for (int i = 0; i < parameterCount; i++)
                {
                    parameters[missingParameterCount + i] = GetBox(data.stack[data.top - i - 1]);
                }

                if (!asyncTask.IsRuning() && !asyncTask.IsComplete())
                {
                    cmd.Execute(parameters, data.heap, asyncTask);
                }

                if (!asyncTask.IsComplete())
                {
                    return MakeRetInfo(false);
                }

                if (!string.IsNullOrEmpty(asyncTask.error))
                {
                    string commandError = asyncTask.error;
                    asyncTask.Reset();
                    AddError(instructionPc, commandError);
                    return MakeRetInfo(true);
                }

                var returnNames = form.retNames;
                int returnCount = returnNames == null ? 0 : returnNames.Count;
                if (returnCount > 0 && (asyncTask.res == null || asyncTask.res.Length < returnCount))
                {
                    string resultError = $"命令 {funcName} 声明 {returnCount} 个返回值，但实际未返回足够结果";
                    asyncTask.Reset();
                    AddError(instructionPc, resultError);
                    return MakeRetInfo(true);
                }

                for (int i = 0; i < returnCount; i++)
                {
                    if (asyncTask.res[i] == null)
                    {
                        string resultError = $"命令 {funcName} 的第 {i + 1} 个返回值为空";
                        asyncTask.Reset();
                        AddError(instructionPc, resultError);
                        return MakeRetInfo(true);
                    }
                }

                PopMany(parameterCount + 1);
                if (!discardReturn)
                {
                    for (int i = 0; i < returnCount; i++)
                    {
                        Push(asyncTask.res[i]);
                    }
                }

                asyncTask.Reset();
                data.p += 2;
                return null;
            }

            if (!ProgramDataForm.DataByName.TryGetValue(funcName, out var func))
            {
                throw new InvalidOperationException($"找不到函数或命令 {funcName}");
            }

            if (parameterCount != func.paramCount)
            {
                throw new InvalidOperationException($"程序 {funcName} 需要 {func.paramCount} 个参数，实际为 {parameterCount} 个");
            }

            int argumentStart = data.top - parameterCount;
            if (data.subInterpret == null)
            {
                var subHeap = new Dictionary<string, BoxDataForm.Data>();
                for (int i = 0; i < parameterCount; i++)
                {
                    subHeap[$"param{i + 1}"] = GetBox(data.stack[argumentStart + i]).DeepCopy();
                }

                data.subInterpret = new InterpretDataForm.Data(
                    -1,
                    new List<BoxDataForm.Data>(),
                    subHeap,
                    func,
                    0,
                    -1,
                    0,
                    null,
                    new List<BoxDataForm.Data>(),
                    data.rootUid == 0 ? data.uid : data.rootUid);
            }

            if (!budget.TryEnterSubProgram())
            {
                AddError(instructionPc, $"程序调用层级超过限制: {funcName}");
                return MakeRetInfo(true);
            }

            RetInfo subResult;
            try
            {
                subResult = data.subInterpret.Interpret(budget);
            }
            finally
            {
                budget.ExitSubProgram();
            }

            if (subResult.errors != null && subResult.errors.Count > 0)
            {
                errors.AddRange(subResult.errors);
                data.subInterpret = null;
                return MakeRetInfo(true);
            }

            if (!subResult.complete)
            {
                return MakeRetInfo(false);
            }

            data.heapTemp?.Clear();
            var completedSubHeap = data.subInterpret.heap;
            for (int i = 0; i < parameterCount; i++)
            {
                string parameterName = $"param{i + 1}";
                if (!completedSubHeap.TryGetValue(parameterName, out var parameterValue) || parameterValue == null)
                {
                    throw new InvalidOperationException($"程序 {funcName} 完成后缺少参数 {parameterName}");
                }

                GetBox(data.stack[argumentStart + i]).Reset(parameterValue);
            }

            PopMany(parameterCount + 1);
            if (!discardReturn)
            {
                Push(subResult.ret ?? CodeHelper.CreateBox());
            }
            data.subInterpret = null;
            data.p += 2;
            return null;
        }

        private bool TryPrepare(out List<string> zCode)
        {
            zCode = null;
            if (data == null)
            {
                AddError(0, "解释器数据为空");
                return false;
            }

            if (data.program == null)
            {
                AddError(data.p, "程序数据为空");
                return false;
            }

            zCode = data.program.zCode;
            if (zCode == null)
            {
                AddError(data.p, "程序 zCode 为空");
                return false;
            }

            if (data.stack == null)
            {
                AddError(data.p, "运行栈为空");
                return false;
            }

            if (data.heap == null)
            {
                AddError(data.p, "运行堆为空");
                return false;
            }

            if (data.p < 0 || data.p > zCode.Count)
            {
                AddError(data.p, $"程序计数器越界: {data.p}/{zCode.Count}");
                return false;
            }

            if (data.top != data.stack.Count - 1)
            {
                AddError(data.p, $"栈顶索引不一致: top={data.top}, Count={data.stack.Count}");
                return false;
            }

            EnsureParseCache(zCode);
            return true;
        }

        private void EnsureParseCache(List<string> zCode)
        {
            if (ReferenceEquals(cachedZCode, zCode) && cachedZCodeCount == zCode.Count)
            {
                return;
            }

            cachedZCode = zCode;
            cachedZCodeCount = zCode.Count;
            opcodeCache = new Op[zCode.Count];
            opcodeCacheValid = new bool[zCode.Count];
            numberCache = new float[zCode.Count];
            numberCacheValid = new bool[zCode.Count];
            integerCache = new int[zCode.Count];
            integerCacheValid = new bool[zCode.Count];
        }

        private bool TryReadOpcode(List<string> zCode, int index, out Op value, out string error)
        {
            value = default;
            error = null;
            if (index < 0 || index >= zCode.Count)
            {
                error = $"操作码位置越界: {index}/{zCode.Count}";
                return false;
            }

            if (opcodeCacheValid[index])
            {
                value = opcodeCache[index];
                return true;
            }

            if (!int.TryParse(zCode[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out int rawValue))
            {
                error = $"无法解析操作码 '{zCode[index]}'";
                return false;
            }

            if (!Enum.IsDefined(typeof(Op), rawValue))
            {
                error = $"未知操作码 {rawValue}";
                return false;
            }

            value = (Op)rawValue;
            opcodeCache[index] = value;
            opcodeCacheValid[index] = true;
            return true;
        }

        private bool TryReadNumber(List<string> zCode, int index, out float value, out string error)
        {
            value = default;
            error = null;
            if (index < 0 || index >= zCode.Count)
            {
                error = $"数值位置越界: {index}/{zCode.Count}";
                return false;
            }

            if (numberCacheValid[index])
            {
                value = numberCache[index];
                return true;
            }

            if (!float.TryParse(zCode[index], NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                float.IsNaN(value) || float.IsInfinity(value))
            {
                error = $"无法解析有限数值 '{zCode[index]}'";
                return false;
            }

            numberCache[index] = value;
            numberCacheValid[index] = true;
            return true;
        }

        private bool TryReadInteger(List<string> zCode, int index, out int value, out string error)
        {
            value = default;
            error = null;
            if (index < 0 || index >= zCode.Count)
            {
                error = $"整数位置越界: {index}/{zCode.Count}";
                return false;
            }

            if (integerCacheValid[index])
            {
                value = integerCache[index];
                return true;
            }

            if (!int.TryParse(zCode[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                error = $"无法解析整数 '{zCode[index]}'";
                return false;
            }

            integerCache[index] = value;
            integerCacheValid[index] = true;
            return true;
        }

        private int GetOperandIndex(List<string> zCode, int instructionPc, Op instruction)
        {
            int operandIndex = instructionPc + 1;
            if (operandIndex >= zCode.Count)
            {
                throw new InvalidOperationException($"{instruction} 指令缺少操作数");
            }

            return operandIndex;
        }

        private int ReadJumpTarget(List<string> zCode, int index)
        {
            if (!TryReadInteger(zCode, index, out int target, out var error))
            {
                throw new FormatException(error);
            }

            if (target < 0 || target > zCode.Count)
            {
                throw new InvalidOperationException($"跳转目标越界: {target}/{zCode.Count}");
            }

            return target;
        }

        private int GetParameterCount()
        {
            RequireStack(1);
            float rawCount = GetNum(data.stack[data.top]);
            if (float.IsNaN(rawCount) || float.IsInfinity(rawCount) || rawCount < 0f ||
                rawCount > int.MaxValue || rawCount != Math.Truncate(rawCount))
            {
                throw new InvalidOperationException($"无效的参数数量: {rawCount}");
            }

            int parameterCount = (int)rawCount;
            if (parameterCount > data.top)
            {
                throw new InvalidOperationException($"参数栈不足: 需要 {parameterCount} 个，实际仅 {data.top} 个");
            }

            return parameterCount;
        }

        private void RequireStack(int count)
        {
            ValidateStackState();
            if (count < 0 || data.stack.Count < count)
            {
                throw new InvalidOperationException($"运行栈不足: 需要 {count} 个值，当前为 {data.stack.Count} 个");
            }
        }

        private void ValidateStackState()
        {
            if (data.stack == null)
            {
                throw new InvalidOperationException("运行栈为空");
            }

            if (data.top != data.stack.Count - 1)
            {
                throw new InvalidOperationException($"栈顶索引不一致: top={data.top}, Count={data.stack.Count}");
            }
        }

        private BoxDataForm.Data Pop()
        {
            RequireStack(1);
            int index = data.top;
            var result = data.stack[index];
            if (result == null)
            {
                throw new InvalidOperationException($"栈位置 {index} 的值为空");
            }

            data.stack.RemoveAt(index);
            data.top--;
            return result;
        }

        private void PopMany(int count)
        {
            RequireStack(count);
            if (count == 0)
            {
                return;
            }

            data.stack.RemoveRange(data.stack.Count - count, count);
            data.top -= count;
        }

        private void Push(BoxDataForm.Data box)
        {
            if (box == null)
            {
                throw new InvalidOperationException("不能向运行栈压入空值");
            }

            ValidateStackState();
            data.stack.Add(box);
            data.top++;
        }

        private BoxDataForm.Data GetBox(BoxDataForm.Data box)
        {
            if (box == null)
            {
                throw new InvalidOperationException("Box 为空");
            }

            string valueName = box.valName;
            if (string.IsNullOrEmpty(valueName))
            {
                EnsureDictionary(box);
                return box;
            }

            if (!data.heap.TryGetValue(valueName, out var heapBox) || heapBox == null)
            {
                heapBox = CodeHelper.CreateBox();
                data.heap[valueName] = heapBox;
            }

            EnsureDictionary(heapBox);
            if (box.str == null)
            {
#if INTERPRETER_DEBUG
                Debug.Log(valueName + " means " + CodeHelper.GetBoxContent(heapBox));
#endif
                return heapBox;
            }

            if (!heapBox.dic.TryGetValue(box.str, out var dictionaryBox) || dictionaryBox == null)
            {
                dictionaryBox = CodeHelper.CreateBox();
                heapBox.dic[box.str] = dictionaryBox;
            }

            EnsureDictionary(dictionaryBox);
            return dictionaryBox;
        }

        private float GetNum(BoxDataForm.Data box)
        {
            return GetBox(box).num;
        }

        private string GetStr(BoxDataForm.Data box)
        {
            box = GetBox(box);
            return box.str ?? box.num.ToString(CultureInfo.InvariantCulture);
        }

        public void Reset()
        {
            asyncTask.Reset();
            errors.Clear();
            opCode = null;
            cachedZCode = null;
            cachedZCodeCount = -1;
            opcodeCache = null;
            opcodeCacheValid = null;
            numberCache = null;
            numberCacheValid = null;
            integerCache = null;
            integerCacheValid = null;
        }

        private BoxDataForm.Data ValuePlus(BoxDataForm.Data left, BoxDataForm.Data right)
        {
            left = GetBox(left);
            right = GetBox(right);
            if (left.str == null && right.str == null)
            {
                return CodeHelper.CreateBoxByNum(left.num + right.num);
            }

            if (left.str != null && right.str == null)
            {
                return CodeHelper.CreateBoxByStr(left.str + right.num.ToString(CultureInfo.InvariantCulture));
            }

            if (left.str == null)
            {
                return CodeHelper.CreateBoxByStr(left.num.ToString(CultureInfo.InvariantCulture) + right.str);
            }

            return CodeHelper.CreateBoxByStr(left.str + right.str);
        }

        private BoxDataForm.Data ValueMinus(BoxDataForm.Data left, BoxDataForm.Data right)
        {
            left = GetBox(left);
            right = GetBox(right);
            if (left.str == null && right.str == null)
            {
                return CodeHelper.CreateBoxByNum(left.num - right.num);
            }

            throw new InvalidOperationException("字符串不支持减法");
        }

        private static bool HasDictionary(BoxDataForm.Data box)
        {
            return box?.dic != null && box.dic.Count > 0;
        }

        private static void EnsureDictionary(BoxDataForm.Data box)
        {
            if (box.dic == null)
            {
                box.dic = new Dictionary<string, BoxDataForm.Data>();
            }
        }

        /// <summary>
        /// 添加解释器运行时错误
        /// </summary>
        private void AddError(int pc, string message, Exception ex = null)
        {
            int lineNumber = 0;
            int columnNumber = 0;
            string sourceLine = null;
            var program = data?.program;

            // 尝试从 zCodeMap 获取原始代码位置
            var zCodeMap = program?.zCodeMap;
            var sourceCode = program?.code;
            if (zCodeMap != null && pc >= 0 && pc < zCodeMap.Count && !string.IsNullOrEmpty(sourceCode))
            {
                int codeIndex = zCodeMap[pc];
                if (codeIndex >= 0 && codeIndex < sourceCode.Length)
                {
                    (lineNumber, columnNumber, sourceLine) = GetLineInfo(sourceCode, codeIndex);
                }
            }

            errors.Add(new InterpretError
            {
                Pc = pc,
                OpCode = opCode ?? default,
                Message = message,
                Exception = ex,
                LineNumber = lineNumber,
                ColumnNumber = columnNumber,
                SourceLine = sourceLine,
                ProgramName = program?.name
            });
        }

        private static (int line, int column, string lineContent) GetLineInfo(string code, int index)
        {
            if (string.IsNullOrEmpty(code) || index < 0 || index >= code.Length)
            {
                return (0, 0, null);
            }

            int line = 1;
            int column = 1;
            int lineStart = 0;
            for (int i = 0; i < index; i++)
            {
                if (code[i] == '\n')
                {
                    line++;
                    column = 1;
                    lineStart = i + 1;
                }
                else
                {
                    column++;
                }
            }

            int lineEnd = code.IndexOf('\n', lineStart);
            string lineContent = lineEnd >= 0
                ? code.Substring(lineStart, lineEnd - lineStart)
                : code.Substring(lineStart);
            return (line, column, lineContent);
        }

        private RetInfo MakeRetInfo(bool complete, BoxDataForm.Data ret = null)
        {
            var info = new RetInfo
            {
                complete = complete,
                ret = ret ?? CodeHelper.CreateBox()
            };
            info.errors.AddRange(errors);
            return info;
        }
    }
}
