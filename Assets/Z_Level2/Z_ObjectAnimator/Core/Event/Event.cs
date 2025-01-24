using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_ObjectAnimator.Base
{
    public class ReturnValue
    {
        public static ReturnValue continueRet = new ReturnValue(Type.Continue);
        public static ReturnValue nextRet = new ReturnValue(Type.Next);

        public ReturnValue(Type type)
        {
            this.type = type;
        }
        public enum Type
        {
            Next,
            Continue,
        }
        public Type type;
    }


    public abstract class Event
    {
        protected int _maxTimes = 0;
        protected int _times = 0;
        public Event(float duration = 0)
        {
            _maxTimes = ((int)(duration / Time.deltaTime)) + 1;
        }
        public virtual ReturnValue Excute()
        {
            

            _times++;
            if (_times >= _maxTimes)
            {
                _times = 0;
                return ReturnValue.nextRet;
            }
            else
            {
                return ReturnValue.continueRet;
            }
        }
        public virtual void ToStart()
        {
            _times = 0;
        }
        public virtual void ToEnd()
        {
            _times = 0;
        }
    }
}
