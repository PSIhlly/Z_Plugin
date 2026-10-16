using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_ObjectAnimator.Base
{

public class WaitFramesEvent : Event
{
    public WaitFramesEvent(int frames):base(0)
    {
        // Frame waits are frame counts, not seconds. Preserve the existing
        // frames + 1 convention even when the scaled clock is paused.
        _maxTimes = Mathf.Max(1, frames + 1);
    }
}
}
