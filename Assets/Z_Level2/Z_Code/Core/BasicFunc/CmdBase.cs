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
        protected abstract Box[] ExecuteInternal(Box[] prm);
        public Box[] Execute(Box[] prm)
        {
            try
            {
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