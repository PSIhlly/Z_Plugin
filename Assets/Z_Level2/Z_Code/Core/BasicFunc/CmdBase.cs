using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using UnityEngine;
using Z_Code.Form;
namespace Z_Code
{

    public abstract class CmdBase
    {
        protected static void Register(CmdBase cmd)
        {
            BaseData.cmdDic[cmd.GetName()] = cmd;
        }
        public virtual void GetUnitChooseCode(Action<string> act, SyntaxNode cur)
        {
            act?.Invoke(CmdDataForm.DataByName[GetName()].defaultCode);
        }

        public abstract string GetName();

        public abstract CmdBase GetNew();
        protected abstract bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask asyncTask);
        public void Execute(BoxDataForm.Data[] prm, Dictionary<string, BoxDataForm.Data> heap, InterpretAsyncTask asyncTask)
        {
            try
            {
                asyncTask.Run();
                for (int i=0; i<prm.Length;i++)
                {
                    if (!string.IsNullOrEmpty(prm[i].valName))
                    {
                        prm[i] = heap[prm[i].valName];
                    }
                    if(i<prm.Length/2)
                    {
                        var tmp = prm[i];
                        prm[i] = prm[prm.Length - i - 1];
                        prm[prm.Length - i - 1] = tmp;
                    }

                }
                if(ExecuteInternal(prm,asyncTask))
                {
                    asyncTask.Complete();
                }
            }
            catch (Exception e)
            {
                asyncTask.error = GetName() + " 执行失败: " + e.Message;
                asyncTask.Complete();
            }
        }
    }
}