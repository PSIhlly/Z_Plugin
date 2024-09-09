using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_ObjectAnimator.Event
{
    public class ReturnValue
    {
        public enum Type
        {
            NextEvnet,
            WaitFrames,
            WaitSeconds
        }
        public float secondsToWait;
        public int framesToWait;
        public Type type;

    }
    
    
    public abstract class Event
    {
        public virtual ReturnValue Excute()
        {
            return new ReturnValue();
        }
    }
}
