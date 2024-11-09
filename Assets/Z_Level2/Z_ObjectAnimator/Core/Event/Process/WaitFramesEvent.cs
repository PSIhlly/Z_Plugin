using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_ObjectAnimator.Event
{

public class WaitFramesEvent : Event
{
    [SerializeField] private int _frames;

    public WaitFramesEvent(int frames)
    {
        _frames = frames;
    }
   
        
    public override ReturnValue Excute()
    {
        var res = new ReturnValue();
        res.type = ReturnValue.Type.WaitFrames;
        res.framesToWait = _frames;
        return res;
    }
}
}