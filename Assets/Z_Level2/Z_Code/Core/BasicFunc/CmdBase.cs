using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        
        public abstract string GetName();

        public abstract CmdBase GetNew();
        protected abstract Box[] ExecuteInternal(Box[] prm);
        public Box[] Execute(Box[] prm, Dictionary<string, Box> heap)
        {
            try
            {
                for(int i=0; i<prm.Length;i++)
                {
                    if (prm[i].valName!=null)
                    {
                        prm[i] = heap[prm[i].valName];
                    }
                }
                var ret = ExecuteInternal(prm);
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