using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

public class CameraInstance : Z_MonoSingleton<CameraInstance>
{
    Transform _camTrs;
    Transform _tarTrs;
    private Vector3 limitMax;
    private Vector3 limitMin;
    public void Register(Vector3 limitMin, Vector3 limitMax)
    {
        this.limitMax = limitMax;
        this.limitMin = limitMin;   
    }


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
    public void LateUpdate()
    {
        tarTrs.position = new Vector3(
            Mathf.Max(Mathf.Min(tarTrs.position.x, limitMax.x), limitMin.x),
            Mathf.Max(Mathf.Min(tarTrs.position.y, limitMax.y), limitMin.y),
            Mathf.Max(Mathf.Min(tarTrs.position.z, limitMax.z), limitMin.z)
            );
    }
}
