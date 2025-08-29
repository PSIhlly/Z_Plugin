using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        public virtual void GetUnitChooseCode(Action<string> act)
        {
            act?.Invoke(CmdDataForm.DataByName[GetName()].defaultCode);
        }

        public abstract string GetName();

        public abstract CmdBase GetNew();
        protected abstract BoxDataForm.Data[] ExecuteInternal(BoxDataForm.Data[] prm, InterpretLock localLock);
        public BoxDataForm.Data[] Execute(BoxDataForm.Data[] prm, Dictionary<string, BoxDataForm.Data> heap, InterpretLock localLock)
        {
            try
            {
                for(int i=0; i<prm.Length;i++)
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
                var ret = ExecuteInternal(prm,localLock);
                return ret==null?null: ret;
            }
            catch (Exception e)
            {
                Debug.LogError(GetName() + " execute fail:" + e);
                return null;
            }
        }
    }
}