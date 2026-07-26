using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Z_Map;
using Z_Math;
[DefaultExecutionOrder(10000)]
public class PerspectiveKeeper : MonoBehaviour
{
    private Vector3 insLastRot;
    private Transform ins;
    public Transform scaleHolder;
    public bool enableFixedYRotation;
    public bool applyYRotationToLocalZ = true;
    public float fixedYRotation;

    [SerializeField]
    private float _deepth;

    public float deepth
    {
        get
        {
            return _deepth;
        }
        set
        {
            _deepth = value;
            if (ins != null)
            {
                UpdateModel();
            }
        }
    }
    public void LateUpdate()
    {
        if (ins == null)
        {
            ins = transform.parent.parent;
            if (ins != null)
            {
                UpdateModel();
            }
            return;
        }

        if (ins.eulerAngles != insLastRot)
        {
            UpdateModel();
            insLastRot = ins.eulerAngles;



            if (enableFixedYRotation)
            {
                transform.eulerAngles = transform.eulerAngles.NewSetY(fixedYRotation);
            }
            else if (applyYRotationToLocalZ)
            {
                transform.eulerAngles = transform.eulerAngles.NewSetY(fixedYRotation);
                transform.localEulerAngles = transform.localEulerAngles.NewSetZ(-ins.eulerAngles.y);
            }
        }

    }
    public void UpdateModel()
    {
        switch (DynamicGlobalSettings.cameraMode)
        {
            case CameraMode.Overhead:
                scaleHolder.position = ins.position + Vector3.up * ins.localScale.y / 2 + Vector3.down * deepth;
                scaleHolder.localScale = Vector3.one;
                transform.eulerAngles = Vector3.right * 90;
                break;
            case CameraMode.Isometric:
                // 直接由已知信息推算 imgTrs 的变换，无需中间节点。
                scaleHolder.position = ins.position + (new Vector3(0, 0.207f, 0)* ins.localScale.y) + new Vector3(0, -1, -1) * deepth;
                transform.eulerAngles = Vector3.zero;
                scaleHolder.localScale = new Vector3(1, 1.414f, 1);
                break;
        }
    }

}
