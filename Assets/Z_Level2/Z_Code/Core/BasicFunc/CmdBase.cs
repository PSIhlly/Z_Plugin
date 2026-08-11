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
        protected static void RegisterAlias(string alias, CmdBase cmd)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("命令别名不能为空", nameof(alias));
            }
            if (cmd == null)
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            BaseData.cmdDic[alias] = cmd;
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
                if (prm == null)
                {
                    throw new ArgumentNullException(nameof(prm));
                }
                if (heap == null)
                {
                    throw new ArgumentNullException(nameof(heap));
                }

                for (int i = 0; i < prm.Length; i++)
                {
                    if (prm[i] == null)
                    {
                        throw new InvalidOperationException($"第 {i + 1} 个命令参数为空");
                    }
                    if (!string.IsNullOrEmpty(prm[i].valName))
                    {
                        if (!heap.TryGetValue(prm[i].valName, out var value) || value == null)
                        {
                            throw new KeyNotFoundException($"找不到命令参数变量 {prm[i].valName}");
                        }
                        prm[i] = value;
                    }
                }

                Array.Reverse(prm);
                if (ExecuteInternal(prm, asyncTask))
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
