//#define INTERPRETER_DEBUG
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using Z_Code.Form;
using Z_Debug;
using Z_Time;
using static Z_Code.Form.InterpretDataForm;
using Stopwatch = System.Diagnostics.Stopwatch;

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
        Discard,
        AddAssign
    }

    internal sealed class InterpretBudget
    {
        private const int MaxSubProgramDepth = 64;
        private int subProgramDepth;
        private long executionStarted, priorExecutionTicks;

        public bool IsTiming { get; private set; }
        public long ElapsedTicks => priorExecutionTicks + Stopwatch.GetTimestamp() - executionStarted;
        public bool HasTimedOut => ElapsedTicks > Stopwatch.Frequency * Interpreter.MaxExecutionSecondsPerFrame;

        public void StartTiming(long elapsedTicks)
        {
            priorExecutionTicks = elapsedTicks;
            executionStarted = Stopwatch.GetTimestamp();
            IsTiming = true;
        }

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
                public BoxDataForm.Data ret;
                public List<InterpretError> errors = new List<InterpretError>();

                public RetInfo() { ret = CodeHelper.CreateBox(); }

                internal RetInfo(bool complete, BoxDataForm.Data ret)
                {
                    this.complete = complete;
                    this.ret = ret ?? CodeHelper.CreateBox();
                }
            }

            public partial class Data
            {
                public int debugId;
                protected Interpreter _interpreter;
                internal Interpreter RuntimeInterpreter => _interpreter ??= new Interpreter(this);
                public bool IsCancelled { get; private set; }

                public void Cancel()
                {
                    if (IsCancelled)
                        return;

                    IsCancelled = true;
                    _interpreter?.Cancel();
                    subInterpret?.Cancel();
                }

                public virtual RetInfo Interpret()
                {
                    return Interpret(new InterpretBudget(Interpreter.MaxInstructionsPerExecution));
                }

                internal RetInfo Interpret(InterpretBudget budget)
                {
                    return RuntimeInterpreter.Interpret(budget);
                }

                public void Reset()
                {
                    _interpreter?.Reset();
                    IsCancelled = false;
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
        private bool isCancelled;

        public bool IsRuning()
        {
            return isRuning;
        }

        public bool IsComplete()
        {
            return isComplete;
        }

        public bool IsCancelled()
        {
            return isCancelled;
        }

        public void Run()
        {
            isComplete = false;
            isRuning = true;
            isCancelled = false;
            error = null;
            res = null;
        }

        public void Cancel()
        {
            isCancelled = true;
            isComplete = true;
            isRuning = false;
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
            isCancelled = false;
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
        // Real execution time only: shared with synchronous subprograms, not
        // time spent suspended across frames or while other events execute.
        public const double MaxExecutionSecondsPerFrame = 1d;

        private int executionFrame = -1, executionPc;
        private long frameExecutionTicks;
        private InterpretBudget activeBudget;

        public InterpretDataForm.Data data;
        private Op? opCode;
        private readonly InterpretAsyncTask asyncTask;
        private BoxDataForm.Data[] queryArguments;
        private bool hasPendingCommand;
        private int pendingCommandPc, pendingParameterCount, pendingReturnCount;
        private string pendingCommandName;
        private bool pendingDiscardReturn;
        private int executionDepth;
        internal bool IsExecuting => executionDepth != 0;

        // Only successful, completed private call frames enter this shared pool.
        // Suspended frames stay with their caller; canceled/error frames retain
        // their cancellation identity for any outstanding command callbacks.
        private sealed class SubProgramContext : InterpretDataForm.Data
        {
            internal SubProgramContext next;
            internal bool CanRecycle => _interpreter == null || !_interpreter.IsExecuting;
            internal SubProgramContext() : base(-1, new List<BoxDataForm.Data>(),
                new Dictionary<string, BoxDataForm.Data>(), null, 0, -1, 0, null,
                new List<BoxDataForm.Data>(), 0) { }
        }
        private const int MaxPooledSubPrograms = 128;
        private static SubProgramContext freeSubPrograms;
        private static int pooledSubProgramCount;
        private static readonly List<string> parameterNames = new List<string>();

        private static string ParameterName(int index)
        {
            while (parameterNames.Count <= index)
                parameterNames.Add("param" + (parameterNames.Count + 1).ToString(CultureInfo.InvariantCulture));
            return parameterNames[index];
        }

        private static SubProgramContext RentSubProgram(ProgramDataForm.Data program, int rootUid)
        {
            var context = freeSubPrograms;
            if (context == null) context = new SubProgramContext();
            else
            {
                freeSubPrograms = context.next;
                context.next = null;
                pooledSubProgramCount--;
            }
            context.program = program;
            context.rootUid = rootUid;
            return context;
        }

        private void ReleaseSubProgram(bool recycle)
        {
            var child = data?.subInterpret;
            if (child == null) return;
            data.subInterpret = null;
            if (!recycle || child.IsCancelled || !(child is SubProgramContext context) || !context.CanRecycle)
            {
                child.Cancel();
                return;
            }
            // Clear containers, never the Boxes they reference: return values and
            // copy-out parameters may now belong to the caller or a command.
            bool retain = pooledSubProgramCount < MaxPooledSubPrograms
                && context.stack.Capacity <= 4096 && context.heap.Count <= 1024 && context.heapTemp.Capacity <= 1024;
            context.Reset();
            context.program = null;
            context.rootUid = 0;
            context.debugId = 0;
            if (retain)
            {
                context.next = freeSubPrograms;
                freeSubPrograms = context;
                pooledSubProgramCount++;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSubProgramPool()
        {
            freeSubPrograms = null;
            pooledSubProgramCount = 0;
            parameterNames.Clear();
        }

        // Only interpreter-created scalar/reference stack values are pooled.
        // Heap/dictionary values and values exported to commands/RetInfo are owned
        // by their recipients and must never return to this pool.
        private sealed class TemporaryBox : BoxDataForm.Data
        {
            internal Interpreter owner;
            internal TemporaryBox next;
            internal int stackReferences;
            internal bool consumed;

            internal TemporaryBox() : base(-1, null, null, 0f, null) { }
        }

        private const int MaxPooledTemporaryBoxes = 128;
        // Interpreters execute on Unity's main thread. Share only free Boxes;
        // in-use ownership and deferred recycling remain interpreter-local.
        private static TemporaryBox freeTemporaryBoxes;
        private static int pooledTemporaryBoxCount;
        private TemporaryBox consumedTemporaryBoxes;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetTemporaryBoxPool()
        {
            freeTemporaryBoxes = null;
            pooledTemporaryBoxCount = 0;
        }

        // Weak keys release all parse buffers when the program/version is no
        // longer referenced. Only primitive parse results are shared, not Box,
        // PC, stack, heap, return values or asynchronous command state.
        // Published compiled lists are immutable; new revisions use new lists.
        private sealed class ParseCache
        {
            internal readonly Op[] opcodes;
            internal readonly float[] numbers;
            internal readonly int[] integers;
            internal readonly bool[] opcodeValid, numberValid, integerValid;
            internal ParseCache(int count)
            {
                opcodes = new Op[count]; numbers = new float[count]; integers = new int[count];
                opcodeValid = new bool[count]; numberValid = new bool[count]; integerValid = new bool[count];
            }
        }
        private static readonly ConditionalWeakTable<List<string>, ParseCache> parseCaches =
            new ConditionalWeakTable<List<string>, ParseCache>();
        private static readonly ConditionalWeakTable<List<string>, ParseCache>.CreateValueCallback createParseCache =
            code => new ParseCache(code.Count);
        private List<string> cachedZCode;
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
            var slice = RunSlice(budget, null, false);
            return MakeRetInfo(slice.complete, slice.ret);
        }

        // Only the public result escapes an invocation. Nested callers consume
        // this value and the child's errors before releasing its context.
        private readonly struct SliceResult
        {
            internal readonly bool complete;
            internal readonly BoxDataForm.Data ret;
            internal SliceResult(bool complete, BoxDataForm.Data ret)
            {
                this.complete = complete;
                this.ret = ret;
            }
        }

        private SliceResult RunSlice(InterpretBudget budget, Interpreter caller, bool discardReturn)
        {
            budget ??= new InterpretBudget(MaxInstructionsPerExecution);
            bool isRoot = !budget.IsTiming;
            if (isRoot)
            {
                if (executionFrame != Time.frameCount)
                {
                    executionFrame = Time.frameCount;
                    frameExecutionTicks = 0;
                }
                budget.StartTiming(frameExecutionTicks);
            }

            var previousBudget = activeBudget;
            activeBudget = budget;
            executionDepth++;
            executionPc = data?.p ?? 0;
#if ENABLE_PROFILER
            bool hasProfile = data?.program != null;
            var profile = hasProfile ? data.program.GetProfilerMarker().Auto() : default;
#endif
            try
            {
                return InterpretCore(budget, caller, discardReturn);
            }
            finally
            {
#if ENABLE_PROFILER
                if (hasProfile) profile.Dispose();
#endif
                if (isRoot) frameExecutionTicks = budget.ElapsedTicks;
                activeBudget = previousBudget;
                executionDepth--;
            }
        }

        private SliceResult InterpretCore(InterpretBudget budget, Interpreter caller, bool discardReturn)
        {
            errors.Clear();
            opCode = null;

            if (data == null || data.IsCancelled)
            {
                return FinishSlice(true);
            }

            if (!TryPrepare(out var zCode))
            {
                return FinishSlice(true);
            }

            int count = zCode.Count;

#if INTERPRETER_DEBUG
            Z_Log.Log(data.debugId + "[Start]" + data.program.code);
#endif

            while (data.p < count)
            {
                if (data.IsCancelled)
                {
                    return FinishSlice(true);
                }

                // One clock read between instructions. Keep the previous PC until this
                // check so a slow command is reported at its own source location.
                if (budget.HasTimedOut)
                {
                    return FinishSlice(true);
                }

                executionPc = data.p;
                if (!budget.TryConsumeInstruction())
                {
                    return FinishSlice(false);
                }

                int instructionPc = data.p;
                if (!TryReadOpcode(zCode, instructionPc, out var parsedOp, out var parseError))
                {
                    AddError(instructionPc, parseError);
                    return FinishSlice(true);
                }

                opCode = parsedOp;

#if INTERPRETER_DEBUG
                Z_Log.Log(data.p + ":" + parsedOp);
                Z_Log.Log("{Current Stacks:}");
                for (int i = 0; i < data.stack.Count; i++)
                {
                    Z_Log.Log("{" + i + " val:" + data.stack[i].valName + " num:" + data.stack[i].num + " str:" + data.stack[i].str + " dic:" + (data.stack[i].dic?.Count ?? 0) + "}");
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

                            Push(RentTemporaryBox(num: number));
                            data.p += 2;
                            break;
                        }
                        case Op.PushStr:
                        {
                            int operandIndex = GetOperandIndex(zCode, instructionPc, parsedOp);
                            Push(RentTemporaryBox(str: zCode[operandIndex]));
                            data.p += 2;
                            break;
                        }
                        case Op.Get:
                        {
                            int operandIndex = GetOperandIndex(zCode, instructionPc, parsedOp);
                            Push(RentTemporaryBox(valName: zCode[operandIndex]));
                            data.p += 2;
                            break;
                        }
                        case Op.Call:
                        case Op.CallDiscard:
                        {
                            int operandIndex = GetOperandIndex(zCode, instructionPc, parsedOp);
                            var callResult = ExecuteCall(zCode[operandIndex], budget, instructionPc,
                                parsedOp == Op.CallDiscard);
                            if (callResult.HasValue)
                            {
                                return callResult.Value;
                            }

                            break;
                        }
                        case Op.Equal:
                        {
                            Pop2(out var left, out var right);
                            left = GetBox(left);
                            right = GetBox(right);
                            if (left.str == null && right.str == null)
                            {
                                PushResult(RentTemporaryBox(num: left.num == right.num ? 1 : 0));
                            }
                            else
                            {
                                PushResult(RentTemporaryBox(num: GetResolvedString(left) == GetResolvedString(right) ? 1 : 0));
                            }

                            data.p++;
                            break;
                        }
                        case Op.Greater:
                        {
                            Pop2(out var left, out var right);
                            PushResult(RentTemporaryBox(num: GetNum(left) > GetNum(right) ? 1 : 0));
                            data.p++;
                            break;
                        }
                        case Op.Less:
                        {
                            Pop2(out var left, out var right);
                            PushResult(RentTemporaryBox(num: GetNum(left) < GetNum(right) ? 1 : 0));
                            data.p++;
                            break;
                        }
                        case Op.NotGreater:
                        {
                            Pop2(out var left, out var right);
                            PushResult(RentTemporaryBox(num: GetNum(left) <= GetNum(right) ? 1 : 0));
                            data.p++;
                            break;
                        }
                        case Op.NotLess:
                        {
                            Pop2(out var left, out var right);
                            PushResult(RentTemporaryBox(num: GetNum(left) >= GetNum(right) ? 1 : 0));
                            data.p++;
                            break;
                        }
                        case Op.NotEqual:
                        {
                            Pop2(out var left, out var right);
                            left = GetBox(left);
                            right = GetBox(right);
                            if (left.str == null && right.str == null)
                            {
                                PushResult(RentTemporaryBox(num: left.num != right.num ? 1 : 0));
                            }
                            else
                            {
                                PushResult(RentTemporaryBox(num: GetResolvedString(left) != GetResolvedString(right) ? 1 : 0));
                            }

                            data.p++;
                            break;
                        }
                        case Op.Take:
                        {
                            Pop2(out var owner, out var member);
                            string key = GetStr(member);
                            var reference = RentTemporaryBox(str: key, valName: owner.valName);
                            PushResult(reference);
                            data.p++;
                            break;
                        }
                        case Op.Assign:
                        {
                            Pop2(out var target, out var source);
                            if (string.IsNullOrEmpty(target.valName))
                            {
                                throw new InvalidOperationException("赋值目标不是变量或成员");
                            }

                            var value = GetBox(source).DeepCopy();
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
                        case Op.AddAssign:
                        {
                            Pop2(out var target, out var right);
                            if (string.IsNullOrEmpty(target.valName))
                            {
                                throw new InvalidOperationException("赋值目标不是变量或成员");
                            }

                            // ArithmeticValues already creates a fully independent
                            // result, including every dictionary child.
                            var value = ArithmeticValues(target, right, Op.Plus, temporaryResult: false);
                            if (target.str != null)
                            {
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
                            Pop2(out var left, out var right);
                            PushResult(ArithmeticValues(left, right, Op.Plus));

                            data.p++;
                            break;
                        }
                        case Op.Positive:
                            PushResult(RentTemporaryBox(num: GetNum(Pop())));
                            data.p++;
                            break;
                        case Op.Minus:
                        {
                            Pop2(out var left, out var right);
                            PushResult(ArithmeticValues(left, right, Op.Minus));

                            data.p++;
                            break;
                        }
                        case Op.Negative:
                            PushResult(RentTemporaryBox(num: -GetNum(Pop())));
                            data.p++;
                            break;
                        case Op.Mul:
                        {
                            Pop2(out var left, out var right);
                            PushResult(ArithmeticValues(left, right, Op.Mul));
                            data.p++;
                            break;
                        }
                        case Op.Div:
                        {
                            Pop2(out var left, out var right);
                            PushResult(ArithmeticValues(left, right, Op.Div));
                            data.p++;
                            break;
                        }
                        case Op.Mod:
                        {
                            Pop2(out var left, out var right);
                            float leftValue = GetNum(left);
                            float rightValue = GetNum(right);
                            if (rightValue == 0f)
                            {
                                throw new DivideByZeroException("取模除数不能为 0");
                            }

                            PushResult(RentTemporaryBox(num: leftValue % rightValue));
                            data.p++;
                            break;
                        }
                        case Op.Not:
                            PushResult(RentTemporaryBox(num: GetNum(Pop()) == 0f ? 1 : 0));
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
                            Pop2(out var owner, out var member);
                            if (string.IsNullOrEmpty(owner.valName) || string.IsNullOrEmpty(member.valName))
                            {
                                throw new InvalidOperationException("成员访问必须使用变量名");
                            }

                            var reference = RentTemporaryBox(str: member.valName, valName: owner.valName);
                            PushResult(reference);
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

                            waitValue.num -= TimeManager.GetDeltaTime();
                            if (waitValue.num > 0f)
                            {
                                Push(waitValue);
                                return FinishSlice(false);
                            }

                            data.p++;
                            break;
                        }
                        case Op.Ret:
                        {
                            var result = GetBox(Pop());
                            data.p++;
                            var slice = FinishSlice(true, discardReturn ? null : result);
                            if (slice.ret is TemporaryBox temporary)
                            {
                                // A command may return an already escaped Box. Transfer only
                                // an exclusive operand owned by this child back to its caller.
                                if (temporary.owner == this && temporary.stackReferences == 0)
                                    temporary.owner = caller;
                                else
                                    ExportBox(temporary);
                            }
                            return slice;
                        }
                        default:
                            throw new InvalidOperationException($"不支持的操作码: {parsedOp}");
                    }
                }
                catch (Exception ex)
                {
                    AddError(instructionPc, $"执行 {parsedOp} 指令失败: {ex.Message}", ex);
                    return FinishSlice(true);
                }
                finally
                {
                    if (consumedTemporaryBoxes != null) ReleaseConsumedTemporaries();
                    opCode = null;
                }
            }

            return FinishSlice(true);
        }

        private SliceResult? ExecuteCall(string funcName, InterpretBudget budget, int instructionPc,
            bool discardReturn)
        {
#if INTERPRETER_DEBUG
            Z_Log.Log(" invoke" + funcName);
#endif
            if (string.IsNullOrEmpty(funcName))
            {
                throw new InvalidOperationException("函数名为空");
            }

            if (hasPendingCommand)
            {
                if (pendingCommandPc != instructionPc || pendingCommandName != funcName)
                    throw new InvalidOperationException("挂起命令与当前调用位置不一致");
                return ResumeCommand(instructionPc);
            }

            // A suspended subprogram owns its original signature and version;
            // new registry entries/edits cannot redirect an existing call.
            int parameterCount = data.subInterpret == null ? GetParameterCount() : data.subInterpret.program.paramCount;
            if (data.subInterpret == null && BaseData.cmdDic.TryGetValue(funcName, out var cmdTemplate))
            {
                var cmd = cmdTemplate.GetNew();
                var form = cmd.GetForm();
                int expectedParameterCount = form.prmNames == null ? 0 : form.prmNames.Count;
                if (parameterCount > expectedParameterCount)
                {
                    throw new InvalidOperationException($"命令 {funcName} 最多接受 {expectedParameterCount} 个参数，实际为 {parameterCount} 个");
                }

                if (cmd is SynchronousQueryCmd query && parameterCount == expectedParameterCount && form.retNames?.Count == 1)
                    return ExecuteQuery(query, funcName, instructionPc, parameterCount, discardReturn);

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
                    parameters[missingParameterCount + i] = ExportBox(GetBox(data.stack[data.top - i - 1]));
                }

                pendingCommandPc = instructionPc;
                pendingCommandName = funcName;
                pendingParameterCount = parameterCount;
                pendingReturnCount = form.retNames == null ? 0 : form.retNames.Count;
                pendingDiscardReturn = discardReturn;
                hasPendingCommand = true;
                cmd.Execute(parameters, data.heap, asyncTask);
                return ResumeCommand(instructionPc);
            }

            var func = data.subInterpret?.program;
            if (func == null && !ProgramDataForm.DataByName.TryGetValue(funcName, out func))
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
                data.subInterpret = RentSubProgram(func.CreateRuntimeSnapshot(), data.rootUid == 0 ? data.uid : data.rootUid);
                try
                {
                    var subHeap = data.subInterpret.heap;
                    for (int i = 0; i < parameterCount; i++)
                        subHeap[ParameterName(i)] = GetBox(data.stack[argumentStart + i]).DeepCopy();
                }
                catch { ReleaseSubProgram(false); throw; }
            }

            if (!budget.TryEnterSubProgram())
            {
                AddError(instructionPc, $"程序调用层级超过限制: {funcName}");
                return FinishSlice(true);
            }

            var child = data.subInterpret.RuntimeInterpreter;
            SliceResult subResult;
            try
            {
                subResult = child.RunSlice(budget, this, discardReturn);
            }
            finally
            {
                budget.ExitSubProgram();
            }

            if (data.IsCancelled)
            {
                return FinishSlice(true);
            }

            if (child.errors.Count > 0)
            {
                errors.AddRange(child.errors);
                ReleaseSubProgram(false);
                return FinishSlice(true);
            }

            if (!subResult.complete)
            {
                return FinishSlice(false);
            }

            bool completed = false;
            try
            {
                data.heapTemp?.Clear();
                var completedSubHeap = data.subInterpret.heap;
                for (int i = 0; i < parameterCount; i++)
                {
                    string parameterName = ParameterName(i);
                    if (!completedSubHeap.TryGetValue(parameterName, out var parameterValue) || parameterValue == null)
                        throw new InvalidOperationException($"程序 {funcName} 完成后缺少参数 {parameterName}");
                    GetBox(data.stack[argumentStart + i]).Reset(parameterValue);
                }
                PopMany(parameterCount + 1);
                if (!discardReturn) Push(subResult.ret ?? CodeHelper.CreateBox());
                completed = true;
            }
            finally { ReleaseSubProgram(completed); }
            data.p += 2;
            return null;
        }

        private SliceResult? ExecuteQuery(SynchronousQueryCmd query, string funcName, int pc, int count, bool discardReturn)
        {
            var buffer = queryArguments;
            if (buffer == null || buffer.Length < count) buffer = new BoxDataForm.Data[count];
            // A query may indirectly invoke script code. Give a nested call its own arguments.
            queryArguments = null;
            try
            {
                int start = data.top - count;
                for (int i = count - 1; i >= 0; i--)
                    buffer[i] = GetBox(data.stack[start + i]);

                BoxDataForm.Data result;
                try { result = query.ExecuteQuery(buffer.AsSpan(0, count), data.heap); }
                catch (Exception ex)
                {
                    AddError(pc, query.GetName() + " 执行失败: " + ex.Message);
                    return FinishSlice(true);
                }
                if (data.IsCancelled) return FinishSlice(true);
                if (result == null)
                {
                    AddError(pc, $"命令 {funcName} 的第 1 个返回值为空");
                    return FinishSlice(true);
                }
                PopMany(count + 1);
                if (!discardReturn) Push(result);
                data.p += 2;
                return null;
            }
            finally
            {
                Array.Clear(buffer, 0, count);
                queryArguments = buffer;
            }
        }

        private SliceResult? ResumeCommand(int instructionPc)
        {
            if (data.IsCancelled) return FinishSlice(true);
            if (!asyncTask.IsComplete()) return FinishSlice(false);
            if (!string.IsNullOrEmpty(asyncTask.error))
            {
                string commandError = asyncTask.error;
                ResetCommand();
                AddError(instructionPc, commandError);
                return FinishSlice(true);
            }
            int returnCount = pendingReturnCount;
            if (returnCount > 0 && (asyncTask.res == null || asyncTask.res.Length < returnCount))
            {
                string resultError = $"命令 {pendingCommandName} 声明 {returnCount} 个返回值，但实际未返回足够结果";
                ResetCommand();
                AddError(instructionPc, resultError);
                return FinishSlice(true);
            }
            for (int i = 0; i < returnCount; i++)
            {
                if (asyncTask.res[i] != null) continue;
                string resultError = $"命令 {pendingCommandName} 的第 {i + 1} 个返回值为空";
                ResetCommand();
                AddError(instructionPc, resultError);
                return FinishSlice(true);
            }
            PopMany(pendingParameterCount + 1);
            if (!pendingDiscardReturn)
                for (int i = 0; i < returnCount; i++) Push(asyncTask.res[i]);
            ResetCommand();
            data.p += 2;
            return null;
        }

        private void ResetCommand()
        {
            hasPendingCommand = false;
            pendingCommandName = null;
            asyncTask.Reset();
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
            if (ReferenceEquals(cachedZCode, zCode))
            {
                return;
            }

            cachedZCode = zCode;
            var shared = parseCaches.GetValue(zCode, createParseCache);
            opcodeCache = shared.opcodes; opcodeCacheValid = shared.opcodeValid;
            numberCache = shared.numbers; numberCacheValid = shared.numberValid;
            integerCache = shared.integers; integerCacheValid = shared.integerValid;
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

        private BoxDataForm.Data RentTemporaryBox(float num = 0f, string str = null, string valName = null)
        {
            var box = freeTemporaryBoxes;
            if (box == null)
                box = new TemporaryBox();
            else
            {
                freeTemporaryBoxes = box.next;
                pooledTemporaryBoxCount--;
            }

            box.owner = this;
            box.next = null;
            box.InitializeTemporaryValue(num, str, valName);
            return box;
        }

        private void ConsumeTemporary(BoxDataForm.Data box)
        {
            if (!(box is TemporaryBox temporary) || temporary.owner != this) return;
            temporary.stackReferences--;
            if (temporary.consumed) return;
            temporary.consumed = true;
            temporary.next = consumedTemporaryBoxes;
            consumedTemporaryBoxes = temporary;
        }

        private void ReleaseConsumedTemporaries()
        {
            while (consumedTemporaryBoxes != null)
            {
                var box = consumedTemporaryBoxes;
                consumedTemporaryBoxes = box.next;
                box.next = null;
                box.consumed = false;
                // Wait pushes its countdown back, and Ret/command arguments may
                // transfer ownership. Both must survive this instruction.
                if (box.owner != this || box.stackReferences > 0) continue;
                box.ClearTemporaryValue();
                // A global free list must not retain the previous interpreter,
                // its heap, pending commands or scene-owned data.
                box.owner = null;
                if (pooledTemporaryBoxCount < MaxPooledTemporaryBoxes)
                {
                    box.next = freeTemporaryBoxes;
                    freeTemporaryBoxes = box;
                    pooledTemporaryBoxCount++;
                }
            }
        }

        private static BoxDataForm.Data ExportBox(BoxDataForm.Data box)
        {
            if (box is TemporaryBox temporary)
                temporary.owner = null;
            return box;
        }

        private void DetachStackTemporaries()
        {
            // Reset/Cancel may run inside a command callback. Keep stack values
            // intact until its caller has finished; detached values are GC-owned.
            if (data?.stack == null) return;
            foreach (var box in data.stack)
                if (box is TemporaryBox temporary && temporary.owner == this)
                    temporary.owner = null;
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
            ConsumeTemporary(result);
            return result;
        }

        private void PopMany(int count)
        {
            RequireStack(count);
            if (count == 0)
            {
                return;
            }

            for (int i = data.stack.Count - count; i < data.stack.Count; i++)
                ConsumeTemporary(data.stack[i]);
            data.stack.RemoveRange(data.stack.Count - count, count);
            data.top -= count;
        }

        private void Pop2(out BoxDataForm.Data first, out BoxDataForm.Data second)
        {
            RequireStack(2);
            var stack = data.stack;
            int top = data.top;
            first = stack[top];
            second = stack[top - 1];
            if (first == null || second == null)
            {
                int emptyIndex = first == null ? top : top - 1;
                throw new InvalidOperationException($"栈位置 {emptyIndex} 的值为空");
            }

            stack.RemoveRange(top - 1, 2);
            data.top = top - 2;
            ConsumeTemporary(first);
            ConsumeTemporary(second);
        }

        private void Push(BoxDataForm.Data box)
        {
            ValidateStackState();
            PushResult(box);
        }

        // Used after Pop/Pop2 has already validated this instruction's stack.
        // No external command/callback may run between that pop and this push.
        private void PushResult(BoxDataForm.Data box)
        {
            if (box == null)
            {
                throw new InvalidOperationException("不能向运行栈压入空值");
            }

            data.stack.Add(box);
            data.top++;
            if (box is TemporaryBox temporary && temporary.owner == this)
                temporary.stackReferences++;
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
                return box;
            }

            if (!data.heap.TryGetValue(valueName, out var heapBox) || heapBox == null)
            {
                heapBox = CodeHelper.CreateBoxByNum(0f);
                data.heap[valueName] = heapBox;
            }

            if (box.str == null)
            {
#if INTERPRETER_DEBUG
                Debug.Log(valueName + " means " + CodeHelper.GetBoxContent(heapBox));
#endif
                return heapBox;
            }

            var dictionary = heapBox.GetOrCreateDictionary();
            if (!dictionary.TryGetValue(box.str, out var dictionaryBox) || dictionaryBox == null)
            {
                dictionaryBox = CodeHelper.CreateBoxByNum(0f);
                dictionary[box.str] = dictionaryBox;
            }

            return dictionaryBox;
        }

        private float GetNum(BoxDataForm.Data box)
        {
            return GetBox(box).num;
        }

        private string GetStr(BoxDataForm.Data box)
        {
            return GetResolvedString(GetBox(box));
        }

        private static string GetResolvedString(BoxDataForm.Data box)
        {
            return box.str ?? box.num.ToString(CultureInfo.InvariantCulture);
        }

        public void Reset()
        {
            ReleaseSubProgram(false);
            DetachStackTemporaries();
            ResetCommand();
            errors.Clear();
            opCode = null;
            cachedZCode = null;
            opcodeCache = null;
            opcodeCacheValid = null;
            numberCache = null;
            numberCacheValid = null;
            integerCache = null;
            integerCacheValid = null;
            executionFrame = -1;
            frameExecutionTicks = 0;
        }

        public void Cancel()
        {
            DetachStackTemporaries();
            asyncTask.Cancel();
            if (!IsExecuting) ReleaseSubProgram(false);
        }

        private BoxDataForm.Data ArithmeticValues(BoxDataForm.Data left, BoxDataForm.Data right, Op operation,
            bool temporaryResult = true)
        {
            return ArithmeticResolved(GetBox(left), GetBox(right), operation, temporaryResult);
        }

        // Resolve each input once, including dictionary children. Broadcast
        // scalars and scalar helpers below already receive resolved values.
        private BoxDataForm.Data ArithmeticResolved(BoxDataForm.Data left, BoxDataForm.Data right, Op operation,
            bool temporaryResult)
        {
            bool leftIsDictionary = HasDictionary(left);
            bool rightIsDictionary = HasDictionary(right);
            if (leftIsDictionary && rightIsDictionary)
            {
                var result = CodeHelper.CreateBox();
                foreach (var pair in left.dic)
                {
                    if (right.dic.TryGetValue(pair.Key, out var rightValue))
                    {
                        result.dic[pair.Key] = ArithmeticResolved(GetBox(pair.Value), GetBox(rightValue), operation,
                            temporaryResult: false);
                    }
                }
                return result;
            }

            if (leftIsDictionary != rightIsDictionary)
            {
                var source = leftIsDictionary ? left : right;
                var result = CodeHelper.CreateBox();
                foreach (var pair in source.dic)
                {
                    result.dic[pair.Key] = leftIsDictionary
                        ? ArithmeticResolved(GetBox(pair.Value), right, operation, temporaryResult: false)
                        : ArithmeticResolved(left, GetBox(pair.Value), operation, temporaryResult: false);
                }
                return result;
            }

            return ScalarArithmetic(left, right, operation, temporaryResult);
        }

        private BoxDataForm.Data ScalarArithmetic(BoxDataForm.Data left, BoxDataForm.Data right, Op operation,
            bool temporaryResult)
        {
            switch (operation)
            {
                case Op.Plus:
                    return ValuePlus(left, right, temporaryResult);
                case Op.Minus:
                    return ValueMinus(left, right, temporaryResult);
                case Op.Mul:
                    return NumberResult(left.num * right.num, temporaryResult);
                case Op.Div:
                    float divisor = right.num;
                    if (divisor == 0f)
                        throw new DivideByZeroException("除数不能为 0");
                    return NumberResult(left.num / divisor, temporaryResult);
                default:
                    throw new InvalidOperationException($"不支持的四则运算: {operation}");
            }
        }

        private BoxDataForm.Data NumberResult(float num, bool temporary) =>
            temporary ? RentTemporaryBox(num: num) : CodeHelper.CreateBoxByNum(num);

        private BoxDataForm.Data StringResult(string str, bool temporary) =>
            temporary ? RentTemporaryBox(str: str) : CodeHelper.CreateBoxByStr(str);

        private BoxDataForm.Data ValuePlus(BoxDataForm.Data left, BoxDataForm.Data right, bool temporaryResult)
        {
            if (left.str == null && right.str == null)
            {
                return NumberResult(left.num + right.num, temporaryResult);
            }

            if (left.str != null && right.str == null)
            {
                return StringResult(left.str + right.ToString(), temporaryResult);
            }

            if (left.str == null)
            {
                return StringResult(left.ToString() + right.str, temporaryResult);
            }

            return StringResult(left.str + right.str, temporaryResult);
        }

        private BoxDataForm.Data ValueMinus(BoxDataForm.Data left, BoxDataForm.Data right, bool temporaryResult)
        {
            if (left.str == null && right.str == null)
            {
                return NumberResult(left.num - right.num, temporaryResult);
            }

            throw new InvalidOperationException("字符串不支持减法");
        }

        private static bool HasDictionary(BoxDataForm.Data box)
        {
            return box?.dic != null && box.dic.Count > 0;
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

        private SliceResult FinishSlice(bool complete, BoxDataForm.Data ret = null)
        {
            // Also check return/yield boundaries: the last instruction or a Cmd
            // that starts asynchronous work can itself take longer than a second.
            if (errors.Count == 0 && data != null && !data.IsCancelled && activeBudget != null
                && activeBudget.HasTimedOut)
            {
                var previousOp = opCode;
                var code = data.program?.zCode;
                if (code != null && TryReadOpcode(code, executionPc, out var timedOutOp, out _))
                    opCode = timedOutOp;
                double seconds = (double)activeBudget.ElapsedTicks / Stopwatch.Frequency;
                AddError(executionPc, $"单帧执行超时：累计耗时 {seconds.ToString("F3", CultureInfo.InvariantCulture)} 秒，"
                    + $"超过 {MaxExecutionSecondsPerFrame.ToString(CultureInfo.InvariantCulture)} 秒限制，事件已终止");
                opCode = previousOp;
                complete = true;
                ret = null;
            }
            if (complete && (data?.IsCancelled == true || errors.Count > 0)) ReleaseSubProgram(false);
            return new SliceResult(complete, ret);
        }

        private RetInfo MakeRetInfo(bool complete, BoxDataForm.Data ret)
        {
            var info = new RetInfo(complete, ret);
            info.errors.AddRange(errors);
            return info;
        }
    }
}
