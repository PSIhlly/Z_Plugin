using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
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
        Sub,
        Get,
        IfFalseJump,
        Jump
    }
    public class Box
    {
        public long num;
        public string str;
        public string valName;
        public Box Copy()
        {
            return new Box()
            {
                num = num,
                str = str
            };
        }
    }
    public class Interpreter
    {

        List<Box> stack = new List<Box>();
        int top=-1;
        Dictionary<string, Box> heap = new Dictionary<string, Box>();
        List<string> zl;
        int p = 0;

        public Interpreter(List<string> zl)
        {
            this.zl = zl;
        }

        public bool Interpret()
        {
            int cnt = zl.Count;
            for(; p<cnt;p++)
            {
                switch((Op)(int.Parse(zl[p])))
                {
                    case Op.PushNum:
                        p++;
                        Push(new Box()
                        {
                            num = long.Parse(zl[p])
                        });
                        break;
                    case Op.PushStr:
                        p++;
                        Push(new Box()
                        {
                            str = zl[p]
                        });
                        break;
                    case Op.Get:
                        p++;
                        var nm = zl[p];
                        if (!heap.ContainsKey(nm))
                        {
                            heap[nm] = new Box();
                        }
                        Push(new Box()
                        {
                            valName = nm
                        });
                        break;
                    case Op.Call:
                        p++;
                        var cmd = BaseData.cmdDic[zl[p]].GetNew();
                        var prm = new Box[cmd.GetPrmCnt()];
                        for(int i=0;i< cmd.GetPrmCnt();i++)
                        {
                            prm[i] = Pop();
                        }
                        var ret=cmd.Execute(prm,heap);
                        for (int i = 0; i < cmd.GetRetCnt(); i++)
                        {
                            Push(ret[i]);
                        }
                        break;
                    case Op.Equal:
                        Push(new Box()
                        {
                            num = GetNum(Pop()) == GetNum(Pop()) ? 1 : 0
                        });
                        break;
                    case Op.Assign:
                        heap[Pop().valName] = Pop().Copy();
                        break;
                    case Op.Plus:
                        Push(new Box()
                        {
                            num = GetNum(Pop()) + GetNum(Pop())
                        });
                        break;
                    case Op.Minus:
                        Push(new Box()
                        {
                            num = GetNum(Pop()) - GetNum(Pop())
                        });
                        break;
                    case Op.Mul:
                        Push(new Box()
                        {
                            num = GetNum(Pop()) * GetNum(Pop())
                        });
                        break;
                    case Op.Div:
                        Push(new Box()
                        {
                            num = GetNum(Pop()) / GetNum(Pop())
                        });
                        break;
                    case Op.Jump:
                        p++;
                        p = int.Parse(zl[p]) - 1;
                        break;
                    case Op.IfFalseJump:
                        p++;
                        if (GetNum(Pop())==0)
                        {
                            p = int.Parse(zl[p]) - 1;
                        }
                        break;
                   /* case Op.Sub:
                        p++;
                        heap[Pop().valName]
                        
                        break;*/


                }


            }
            return true;
        }
        private Box Pop()
        {
           var res= stack[top];
            stack.RemoveAt(top);
            top--;
            return res;
        }
        private void Push(Box box)
        {
            stack.Add(box);
            top++;
        }
        private long GetNum(Box box)
        {
            if(box.valName!=null)
            {
                return heap[box.valName].num;
            }
            return box.num;
        }
        private string GetStr(Box box)
        {
            if (box.valName != null)
            {
                return heap[box.valName].str;
            }
            return box.str;
        }
    }


}
