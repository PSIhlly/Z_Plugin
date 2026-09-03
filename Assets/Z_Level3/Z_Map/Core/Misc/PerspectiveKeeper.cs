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
    private bool needsInitialUpdate = true;
    public Transform scaleHolder;
    public bool enableFixedYRotation;
    public bool applyYRotationToLocalZ = true;
    public float fixedYRotation;
    public bool enableFixedZRotation0;
    public bool enableFixedXRotation0;

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

    private void OnEnable()
    {
        // Instances are pooled and may be re-enabled with a cached rotation from
        // their previous owner. Force the first refresh after every reuse.
        needsInitialUpdate = true;
    }

    /// <summary>
    /// Applies the current instance transform immediately. This is used when a
    /// command changes an object's rotation during LateUpdate, after this
    /// component's own LateUpdate pass has already been skipped for the frame.
    /// </summary>
    public void RefreshNow()
    {
        if (transform.parent == null || transform.parent.parent == null)
            return;

        ins = transform.parent.parent;
        UpdateModel();
        insLastRot = ins.eulerAngles;
        ApplyRotation();
        needsInitialUpdate = false;
    }

    public void LateUpdate()
    {
        if (ins == null)
        {
            ins = transform.parent.parent;
            if (ins != null)
            {
                UpdateModel();
                insLastRot = ins.eulerAngles;
                ApplyRotation();
                needsInitialUpdate = false;
            }
            return;
        }

        if (needsInitialUpdate || ins.eulerAngles != insLastRot)
        {
            UpdateModel();
            insLastRot = ins.eulerAngles;
            ApplyRotation();
            needsInitialUpdate = false;
        }
    }

    private void ApplyRotation()
    {
            if (applyYRotationToLocalZ)
            {
                transform.eulerAngles = transform.eulerAngles.NewSetY(fixedYRotation);
                transform.localEulerAngles = transform.localEulerAngles.NewSetZ(-ins.eulerAngles.y);
            }

            if (enableFixedYRotation)
            {
                transform.eulerAngles = transform.eulerAngles.NewSetY(fixedYRotation);
            }
            if (enableFixedXRotation0)
            {
                transform.eulerAngles = transform.eulerAngles.NewSetX(0);
            }
            if (enableFixedZRotation0)
            {
                transform.eulerAngles = transform.eulerAngles.NewSetZ(0);
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
