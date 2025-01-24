using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

public class CameraInstance : Z_MonoSingleton<CameraInstance>
{
    Transform _camTrs;
    Transform _tarTrs;

    public Transform camTrs
    {
        get
        {
            if(_camTrs==null)
            {
                _camTrs = Camera.main.transform;
            }
            return _camTrs;
        }
    }
    public Transform tarTrs
    {
        get
        {
            if (_tarTrs == null)
            {
                _tarTrs = transform;
            }
            return _tarTrs;
        }
    }
}
