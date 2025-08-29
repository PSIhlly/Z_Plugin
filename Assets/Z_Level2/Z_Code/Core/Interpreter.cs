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
        Jump
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
                        _interpreter = new Interpreter(program);
                    }
                    return _interpreter.Interpret();
                }
            }

        }
    }
    public class InterpretLock
    {
        Interpreter interpreter;
        public InterpretLock(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }
        private bool isLocked;
        public bool IsLocked()
        {
            return isLocked;
        }
        public void Lock()
        {
            isLocked = true;
        }
        public void Unlock()
        {
            isLocked = false;
            interpreter.data.p++;
        }
    }

    public class Interpreter
    {
        public InterpretDataForm.Data data;
        public const bool DEBUG = false;
        InterpretLock localLock;

        public Interpreter(ProgramDataForm.Data program)
        {
            data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(), program, 0, -1);
            localLock = new InterpretLock(this);
        }

        public bool Interpret()
        {
            if (localLock.IsLocked())
            {
                return false;
            }

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
                        Push(new BoxDataForm.Data(-1, "", "", float.Parse(data.program.zCode[data.p])));
                        break;
                    case Op.PushStr:
                        data.p++;
                        Push(new BoxDataForm.Data(-1, data.program.zCode[data.p], "", 0));
                        break;
                    case Op.Get:
                        data.p++;
                        var nm = data.program.zCode[data.p];
                        if (!data.heap.ContainsKey(nm))
                        {
                            data.heap[nm] = new BoxDataForm.Data(-1, "", "", 0);
                        }
                        Push(new BoxDataForm.Data(-1, "", nm, 0));
                        break;
                    case Op.Call:
                        data.p++;
                        var cmd = BaseData.cmdDic[data.program.zCode[data.p]].GetNew();
                        var form = cmd.GetForm();
                        var prm = new BoxDataForm.Data[form.prmNames == null ? 0 : form.prmNames.Count];
                        for (int i = 0; i < prm.Length; i++)
                        {
                            prm[i] = Pop();
                        }
                        var ret = cmd.Execute(prm, data.heap, localLock);
 
                        for (int i = 0; i < (form.retNames == null ? 0 : form.retNames.Count); i++)
                        {
                            Push(ret[i]);
                        }
                        if (localLock.IsLocked())
                        {
                            return false;
                        }
                        break;
                    case Op.Equal:
                        Push(new BoxDataForm.Data(-1, "", "", GetNum(Pop()) == GetNum(Pop()) ? 1 : 0));
                        break;
                    case Op.NotEqual:
                        Push(new BoxDataForm.Data(-1, "", "", GetNum(Pop()) != GetNum(Pop()) ? 1 : 0));
                        break;
                    case Op.Assign:
                        data.heap[Pop().valName] = Pop().Copy();
                        break;
                    case Op.Plus:
                        Push(new BoxDataForm.Data(-1, "", "", GetNum(Pop()) + GetNum(Pop())));
                        break;
                    case Op.Minus:
                        Push(new BoxDataForm.Data(-1, "", "", GetNum(Pop()) - GetNum(Pop())));
                        break;
                    case Op.Mul:
                        Push(new BoxDataForm.Data(-1, "", "", GetNum(Pop()) * GetNum(Pop())));
                        break;
                    case Op.Div:
                        Push(new BoxDataForm.Data(-1, "", "", GetNum(Pop()) / GetNum(Pop())));
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
    }


}
