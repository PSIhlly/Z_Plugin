using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Z_Code.Form;
using Z_Debug;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
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
        NotLess
    }
    namespace Form
    {

        public static partial class InterpretDataForm
        {
            public partial class Data
            {
                protected Interpreter _interpreter;
                public virtual bool Interpret()
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

        public bool Interpret()
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
                        var cmd = BaseData.cmdDic[data.program.zCode[data.p + 1]].GetNew();
                        var form = cmd.GetForm();
                        var prm = new BoxDataForm.Data[form.prmNames == null ? 0 : form.prmNames.Count];
                        for (int i = 0; i < prm.Length; i++)
                        {
                            prm[i] = GetBox(data.stack[data.top - i]);
                        }
                        if (!asyncTask.IsRuning() && !asyncTask.IsComplete())
                        {
                            cmd.Execute(prm, data.heap, asyncTask);
                        }

                        if (asyncTask.IsComplete())
                        {
                            //Delay
                            data.p++;
                            for (int i = 0; i < prm.Length; i++)
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
                            return false;
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
                        realBox = GetBox(box);
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
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) + GetNum(Pop())));
                        break;
                    case Op.Positive:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop())));
                        break;
                    case Op.Minus:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop()) - GetNum(Pop())));
                        break;
                    case Op.Negative:
                        Push(CodeHelper.CreateBoxByNum(GetNum(Pop())*-1f));
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
                    /* case Op.Sub:
                         p++;
                         heap[Pop().valName]

                         break;*/
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
                            return false;
                        }
                        break;
                    default:
                        Z_Log.Log($"op:{int.Parse(data.program.zCode[data.p])} not found£¡£¡");
                        break;

                }


            }
            return true;
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
