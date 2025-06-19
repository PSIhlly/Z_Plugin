using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_Code
{

    public abstract class CmdBase
    {
        public abstract int GetPrmCnt();
        public abstract int GetRetCnt();
        public abstract string GetName();

        public static void Register<T>(T cmd) where T : CmdBase
        {
            BaseData.cmdDic[cmd.GetName()] = cmd;
        }
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