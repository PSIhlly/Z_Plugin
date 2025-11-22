using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using Z_Code.Form;
using Z_Debug;
namespace Z_Code
{
    public enum Op
    {
        PushNum,
        PushStr,
        Call,
        Plus,
        Minus,
        Mul,
        Div,
        Assign,
        Equal,
        NotEqual,
        Sub,
        Get,
        IfFalseJump,
        Jump,
        Wait
    }
    namespace Form
    {

        public static partial class InterpretDataForm
        {
            public partial class Data
            {
                Interpreter _interpreter;
                public bool Interpret()
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
        Interpreter interpreter;
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
                        Push(new BoxDataForm.Data(-1, null, null, float.Parse(data.program.zCode[data.p])));
                        break;
                    case Op.PushStr:
                        data.p++;
                        Push(new BoxDataForm.Data(-1, data.program.zCode[data.p], null, 0));
                        break;
                    case Op.Get:
                        data.p++;
                        var nm = data.program.zCode[data.p];
                        if (!data.heap.ContainsKey(nm))
                        {
                            data.heap[nm] = new BoxDataForm.Data(-1, null, null, 0);
                        }
                        Push(new BoxDataForm.Data(-1, null, nm, 0));
                        break;
                    case Op.Call:
                        var cmd = BaseData.cmdDic[data.program.zCode[data.p + 1]].GetNew();
                        var form = cmd.GetForm();
                        var prm = new BoxDataForm.Data[form.prmNames == null ? 0 : form.prmNames.Count];
                        for (int i = 0; i < prm.Length; i++)
                        {
                            prm[i] = data.stack[data.top - i];
                        }
                        if (!asyncTask.IsRuning()&&!asyncTask.IsComplete())
                        {
                            cmd.Execute(prm, data.heap, asyncTask);
                        }

                        if(asyncTask.IsComplete())
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
                        Push(new BoxDataForm.Data(-1, null, null, GetNum(Pop()) == GetNum(Pop()) ? 1 : 0));
                        break;
                    case Op.NotEqual:
                        Push(new BoxDataForm.Data(-1, null, null, GetNum(Pop()) != GetNum(Pop()) ? 1 : 0));
                        break;
                    case Op.Assign:
                        data.heap[Pop().valName] = Pop().Copy();
                        break;
                    case Op.Plus:
                        Push(new BoxDataForm.Data(-1, null, null, GetNum(Pop()) + GetNum(Pop())));
                        break;
                    case Op.Minus:
                        Push(new BoxDataForm.Data(-1, null, null, GetNum(Pop()) - GetNum(Pop())));
                        break;
                    case Op.Mul:
                        Push(new BoxDataForm.Data(-1, null, null, GetNum(Pop()) * GetNum(Pop())));
                        break;
                    case Op.Div:
                        Push(new BoxDataForm.Data(-1, null, null, GetNum(Pop()) / GetNum(Pop())));
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
                        var box = Pop();
                        box.num -= Time.deltaTime;
                        if(box.num > 0)
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
        private float GetNum(BoxDataForm.Data box)
        {
            if (!string.IsNullOrEmpty(box.valName))
            {
                return data.heap[box.valName].num;
            }
            return box.num;
        }
        private string GetStr(BoxDataForm.Data box)
        {
            if (!string.IsNullOrEmpty(box.valName))
            {
                return data.heap[box.valName].str;
            }
            return box.str;
        }
        public void Reset()
        {
            asyncTask.Reset();
        }
    }


}
