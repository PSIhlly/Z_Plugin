using System;
using System.Collections.Generic;
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
        Ret
    }
    namespace Form
    {

        public static partial class InterpretDataForm
        {
            public class RetInfo
            {
                public bool complete;
                public BoxDataForm.Data ret = CodeHelper.CreateBox();
            }
            public partial class Data
            {
                protected Interpreter _interpreter;
                public virtual RetInfo Interpret()
                {
                    if (_interpreter == null)
                    {
                        _interpreter = new Interpreter(this);
                    }
                    return _interpreter.Interpret();
                }
                public void Reset()
                {
                    _interpreter.Reset();

                    p = 0;
                    stack.Clear();
                    top = -1;
                    heap.Clear();
                }
            }

        }
    }
    public class InterpretAsyncTask
    {
        public readonly Interpreter interpreter;
        public BoxDataForm.Data[] res;
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
        }
    }

    public class Interpreter
    {
        public InterpretDataForm.Data data;
        public const bool DEBUG = false;
        InterpretAsyncTask asyncTask;

        public Interpreter(InterpretDataForm.Data interpret)
        {
            data = interpret;
            asyncTask = new InterpretAsyncTask(this);
        }

        public RetInfo Interpret()
        {

            int cnt = data.program.zCode.Count;
            BoxDataForm.Data box = null;
            BoxDataForm.Data realBox = null;

            for (; data.p < cnt; data.p++)
            {
                if (DEBUG)
                {
                    Z_Log.Log(data.p + ":" + (data.program.zCode[data.p]));
                }
                switch ((Op)(int.Parse(data.program.zCode[data.p])))
                {
                    case Op.PushNum:
                        data.p++;
                        Push(CodeHelper.CreateBoxByNum(float.Parse(data.program.zCode[data.p])));
                        break;
                    case Op.PushStr:
                        data.p++;
                        Push(CodeHelper.CreateBoxByStr(data.program.zCode[data.p]));
                        break;
                    case Op.Get:
                        data.p++;
                        var nm = data.program.zCode[data.p];
                        Push(CodeHelper.CreateBoxByVal(nm));
                        break;
                    case Op.Call:
                        string funcName = data.program.zCode[data.p + 1];
                        try
                        {
                            if (BaseData.cmdDic.ContainsKey(funcName))
                            {
                                var cmd = BaseData.cmdDic[funcName].GetNew();
                                var form = cmd.GetForm();
                                var prmCount = (int)GetNum(data.stack[data.top]);
                                var prm = new BoxDataForm.Data[prmCount];
                                for (int i = 0; i < prm.Length; i++)
                                {
                                    prm[i] = GetBox(data.stack[data.top - i - 1]);
                                }
                                if (!asyncTask.IsRuning() && !asyncTask.IsComplete())
                                {
                                    cmd.Execute(prm, data.heap, asyncTask);
                                }

                                if (asyncTask.IsComplete())
                                {
                                    //Delay
                                    data.p++;
                                    for (int i = 0; i <= prm.Length; i++)
                                    {
                                        Pop();
                                    }
                                    for (int i = 0; i < (form.retNames == null ? 0 : form.retNames.Count); i++)
                                    {
                                        Push(asyncTask.res[i]);
                                    }
                                    asyncTask.Reset();
                                }
                                else
                                {
                                    return new RetInfo();
                                }

                            }
                            else if (ProgramDataForm.DataByName.ContainsKey(funcName))
                            {
                                var func = ProgramDataForm.DataByName[funcName];

                                var prmCount = (int)GetNum(data.stack[data.top]);
                                var prm = new BoxDataForm.Data[prmCount];
                                var newHeap = new Dictionary<string, BoxDataForm.Data>();
                                for (int i = 0; i < prm.Length; i++)
                                {
                                    newHeap[$"param{i + 1}"] = GetBox(data.stack[data.top - i - 1]).Copy();
                                }
                                if (data.subInterpret == null)
                                {
                                    data.subInterpret = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), newHeap, func, 0, -1, 0, null);
                                }
                                var ret = data.subInterpret.Interpret();

                                if (ret.complete)
                                {
                                    //Delay
                                    data.p++;
                                    for (int i = 0; i < prm.Length; i++)
                                    {
                                        GetBox(data.stack[data.top - i - 1]).Reset(data.subInterpret.heap[$"param{i + 1}"]);
                                    }
                                    for (int i = 0; i <= prm.Length; i++)
                                    {
                                        Pop();
                                    }
                                    Push(ret.ret);

                                    asyncTask.Reset();
                                    data.subInterpret = null;
                                }
                                else
                                {
                                    return new RetInfo();
                                }
                            }
                            else
                            {
                                throw new Exception("can't find");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(funcName + " " + e);
                        }

                        break;
                    case Op.Equal:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) == GetNum(Pop()) ? 1 : 0));
                        break;
                    case Op.Greater:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) > GetNum(Pop()) ? 1 : 0));
                        break;
                    case Op.Less:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) < GetNum(Pop()) ? 1 : 0));
                        break;
                    case Op.NotGreater:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) <= GetNum(Pop()) ? 1 : 0));
                        break;
                    case Op.NotLess:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) >= GetNum(Pop()) ? 1 : 0));
                        break;
                    case Op.NotEqual:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) != GetNum(Pop()) ? 1 : 0));
                        break;
                    case Op.Take:
                        box = Pop();
                        var key = GetStr(Pop());
                        box = CodeHelper.CreateBoxByVal(box.valName);
                        box.str = key;
                        Push(box);
                        break;
                    case Op.Assign:
                        box = Pop();
                        realBox = GetBox(box);
                        if (box.str != null)
                        {
                            data.heap[box.valName].dic[box.str] = GetBox(Pop()).Copy();
                        }
                        else
                        {
                            data.heap[box.valName] = GetBox(Pop()).Copy();
                        }
                        break;
                    case Op.Plus:
                        var box1 = Pop();
                        var box2 = Pop();
                        if (box1.str == null && box2.str == null)
                        {

                            Push(CodeHelper.CreateBoxByNum(GetNum(box1) + GetNum(box2)));
                        }
                        else if (box1.str != null && box2.str == null)
                        {
                            Push(CodeHelper.CreateBoxByStr(GetStr(box1) + GetNum(box2)));
                        }
                        else if (box1.str == null && box2.str != null)
                        {
                            Push(CodeHelper.CreateBoxByStr(GetNum(box1) + GetStr(box2)));
                        }
                        else
                        {
                            Push(CodeHelper.CreateBoxByStr(GetStr(box1) + GetStr(box2)));
                        }

                        break;
                    case Op.Positive:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop())));
                        break;
                    case Op.Minus:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) - GetNum(Pop())));
                        break;
                    case Op.Negative:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) * -1f));
                        break;
                    case Op.Mul:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) * GetNum(Pop())));
                        break;
                    case Op.Div:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) / GetNum(Pop())));
                        break;
                    case Op.Jump:
                        data.p++;
                        data.p = int.Parse(data.program.zCode[data.p]) - 1;
                        break;
                    case Op.IfFalseJump:
                        data.p++;
                        if (GetNum(Pop()) == 0)
                        {
                            data.p = int.Parse(data.program.zCode[data.p]) - 1;
                        }
                        break;
                    case Op.Sub://take same
                        box = Pop();
                        string paramName = Pop().valName;
                        box = CodeHelper.CreateBoxByVal(box.valName);
                        box.str = paramName;
                        Push(box);
                        break;
                    case Op.Wait:
                        box = Pop();
                        if (box.valName != null && box.num == 0)
                        {
                            box.num = GetBox(box).num;
                        }
                        box.num -= Time.deltaTime;
                        if (box.num > 0)
                        {
                            Push(box);
                            return new RetInfo();
                        }
                        break;
                    case Op.Ret:
                        box = Pop();
                        realBox = GetBox(box);
                        return new RetInfo()
                        {
                            ret = realBox
                        };
                    default:
                        Z_Log.Log($"op:{int.Parse(data.program.zCode[data.p])} not found£¡£¡");
                        break;

                }


            }


            return new RetInfo() { complete = true };
        }

        private BoxDataForm.Data Pop()
        {
            var res = data.stack[data.top];
            data.stack.RemoveAt(data.top);
            data.top--;
            return res;
        }
        private void Push(BoxDataForm.Data box)
        {
            data.stack.Add(box);
            data.top++;
        }
        private BoxDataForm.Data GetBox(BoxDataForm.Data box)
        {
            if (!string.IsNullOrEmpty(box.valName))
            {
                if (!data.heap.ContainsKey(box.valName))
                {
                    data.heap[box.valName] = CodeHelper.CreateBox();
                }
                if (box.str != null)
                {
                    if (!data.heap[box.valName].dic.ContainsKey(box.str))
                    {
                        data.heap[box.valName].dic[box.str] = CodeHelper.CreateBox();
                    }
                    return data.heap[box.valName].dic[box.str];
                }
                else
                {
                    return data.heap[box.valName];
                }
            }
            return box;
        }
        private float GetNum(BoxDataForm.Data box)
        {
            box = GetBox(box);
            return box.num;
        }
        private string GetStr(BoxDataForm.Data box)
        {
            box = GetBox(box);
            if (box.str == null)
            {
                return box.num.ToString();
            }
            return box.str;
        }
        public void Reset()
        {
            asyncTask.Reset();
        }

    }


}
