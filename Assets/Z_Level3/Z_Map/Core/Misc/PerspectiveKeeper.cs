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
            ins = transform.parent;
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
        MapManager.instance.utilCtrl.SetPerspectiveModel(ins, transform, _deepth);
    }

}
