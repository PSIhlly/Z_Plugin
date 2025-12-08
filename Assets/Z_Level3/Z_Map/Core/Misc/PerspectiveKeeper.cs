using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Map;
using Z_Math;

public class PerspectiveKeeper : MonoBehaviour
{
    private Vector3 insLastRot;
    private Transform ins;
    public bool enableFixedYRotation;
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
            _deepth= value;
            if (ins != null)
            {
                UpdateModel();
            }
        }
    }
    public void Update()
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
        }

    }
    public void UpdateModel()
    {
        MapManager.instance.utilCtrl.SetPerspectiveModel(transform.parent, transform, _deepth);
    }

}
