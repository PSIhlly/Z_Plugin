using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

public class CameraInstance : Z_MonoSingleton<CameraInstance>
{
    Camera _cam;
    public Light globalLight;
    Transform _camTrs;
    Transform _tarTrs;
    private Vector3 limitMax;
    private Vector3 limitMin;
    private float orthographicSizeMax=7;
    private float orthographicSizeMin=7;
    public bool followsPlayer { get; private set; } = true;
    private uint scriptedMoveVersion;

    public uint BeginScriptedMove()
    {
        followsPlayer = false;
        return ++scriptedMoveVersion;
    }

    public bool HasScriptedControl(uint version) =>
        isActiveAndEnabled && !followsPlayer && version == scriptedMoveVersion;

    public void SetScriptedPosition(uint version, Vector3 position)
    {
        if (HasScriptedControl(version))
            tarTrs.position = ClampPosition(position);
    }

    public void RestorePlayerFollow()
    {
        ++scriptedMoveVersion;
        followsPlayer = true;
    }

    public void FollowPlayer(Vector3 position)
    {
        if (followsPlayer)
            tarTrs.position = ClampPosition(position);
    }

    public Vector3 ClampPosition(Vector3 position) => new Vector3(
        Mathf.Clamp(position.x, limitMin.x, limitMax.x),
        Mathf.Clamp(position.y, limitMin.y, limitMax.y),
        Mathf.Clamp(position.z, limitMin.z, limitMax.z));

    private void OnDisable() => RestorePlayerFollow();

    public void Register(Vector3 limitMin, Vector3 limitMax, float orthographicSizeMin, float orthographicSizeMax,float defaultographicSize=4)
    {
        RestorePlayerFollow();
        this.limitMax = limitMax;
        this.limitMin = limitMin;
        this.orthographicSizeMax = orthographicSizeMax;
        this.orthographicSizeMin = orthographicSizeMin;
        cam.orthographicSize = defaultographicSize;
    }
    public Camera cam
    {
        get
        {
            if (_cam == null)
            {
                _cam = gameObject.GetComponentInChildren<Camera>();
            }
            return _cam;
        }
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
        tarTrs.position = ClampPosition(tarTrs.position);
        cam.orthographicSize = Mathf.Max(Mathf.Min(cam.orthographicSize, orthographicSizeMax), orthographicSizeMin); 
    }
}
