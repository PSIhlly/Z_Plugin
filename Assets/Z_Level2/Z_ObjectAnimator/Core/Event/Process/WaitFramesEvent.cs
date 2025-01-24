using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_ObjectAnimator.Base
{

public class WaitFramesEvent : Event
{
    public WaitFramesEvent(int frames):base(frames* Time.deltaTime)
    {

    }
}
}