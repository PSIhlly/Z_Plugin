using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_ObjectAnimator.Event
{

public class WaitSecondEvent : Event
{
    [SerializeField] private float _seconds;

    public WaitSecondEvent(float seconds)
    {
        _seconds = seconds;
    }
   
        
    public override ReturnValue Excute()
    {
        var res = new ReturnValue();
        res.type = ReturnValue.Type.WaitSeconds;
        res.secondsToWait = _seconds;
        return res;
    }
}
}