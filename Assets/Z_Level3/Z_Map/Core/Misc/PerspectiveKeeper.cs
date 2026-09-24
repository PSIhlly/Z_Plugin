using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Z_Map;
using Z_Math;
[DefaultExecutionOrder(10000)]
public class PerspectiveKeeper : MonoBehaviour
{
    private const float SquareDiagonalPitch = 45f;
    private const float MinimumCubeSize = 0.0001f;
    private const string SpherePrefabName = "MapPrefab$sphere";

    private Vector3 insLastRot;
    private Vector3 insLastScale;
    private Transform ins;
    private bool needsInitialUpdate = true;
    private bool hasResolvedSphere;
    private bool isSphere;
    private CameraMode lastCameraMode;
    public Transform scaleHolder;
    public bool enableFixedYRotation;
    public bool applyYRotationToLocalZ = true;
    public float fixedYRotation;
    public bool enableFixedZRotation0;

    [FormerlySerializedAs("enableFixedXRotation0")]
    [Tooltip("Fix the image surface to the current camera mode's base pitch.")]
    public bool enableFixedXRotationDefault;

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
        insLastScale = ins.lossyScale;
        lastCameraMode = DynamicGlobalSettings.cameraMode;
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
                insLastScale = ins.lossyScale;
                lastCameraMode = DynamicGlobalSettings.cameraMode;
                ApplyRotation();
                needsInitialUpdate = false;
            }
            return;
        }

        if (needsInitialUpdate
            || ins.eulerAngles != insLastRot
            || ins.lossyScale != insLastScale
            || DynamicGlobalSettings.cameraMode != lastCameraMode)
        {
            UpdateModel();
            insLastRot = ins.eulerAngles;
            insLastScale = ins.lossyScale;
            lastCameraMode = DynamicGlobalSettings.cameraMode;
            ApplyRotation();
            needsInitialUpdate = false;
        }
    }

    private void ApplyRotation()
    {
        float pitch = enableFixedXRotationDefault
            ? GetBasePitch()
            : transform.eulerAngles.x;
        float yaw = applyYRotationToLocalZ || enableFixedYRotation
            ? fixedYRotation
            : transform.eulerAngles.y;
        float roll = applyYRotationToLocalZ && !enableFixedZRotation0
            ? -ins.eulerAngles.y
            : 0f;

        // Compose the character/object direction as a rotation inside the
        // already tilted image plane. Mutating local Euler Z after setting a
        // world-space pitch mixes the carrier's Y rotation into the plane normal.
        Quaternion surfaceRotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = surfaceRotation * Quaternion.AngleAxis(roll, Vector3.forward);
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
                // Cubes use the Y-Z diagonal; spheres use a diameter-sized square.
                scaleHolder.position = ins.position + new Vector3(0, 1, -1) * deepth;
                transform.eulerAngles = Vector3.right * GetIsometricPitch();
                scaleHolder.localScale = IsSphere()
                    ? GetSphereRendererScale()
                    : GetDiagonalRendererScale();
                break;
        }
    }

    private Vector3 GetDiagonalRendererScale()
    {
        Vector3 cubeSize = ins.lossyScale;
        float height = Mathf.Abs(cubeSize.y);
        float depth = Mathf.Abs(cubeSize.z);
        float diagonal = Mathf.Sqrt(height * height + depth * depth);

        float heightScale = height > MinimumCubeSize
            ? diagonal / height
            : 1f;
        return new Vector3(1f, heightScale, 1f);
    }

    private float GetIsometricPitch()
    {
        if (IsSphere())
            return SquareDiagonalPitch;

        Vector3 cubeSize = ins.lossyScale;
        float height = Mathf.Abs(cubeSize.y);
        float depth = Mathf.Abs(cubeSize.z);
        if (height <= MinimumCubeSize && depth <= MinimumCubeSize)
            return SquareDiagonalPitch;

        return Mathf.Atan2(depth, height) * Mathf.Rad2Deg;
    }

    private Vector3 GetSphereRendererScale()
    {
        // The primitive sphere has diameter 1. Do not use the gameplay Collider
        // radius (characters reduce it independently of their visual size).
        // Cancel the primitive's nonuniform scale on all three axes BEFORE the
        // image rotation, so rotating the square cannot turn it into a shear.
        Vector3 size = ins.localScale;
        float diameter = Mathf.Max(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        return new Vector3(
            Mathf.Abs(size.x) > MinimumCubeSize ? diameter / Mathf.Abs(size.x) : 1f,
            Mathf.Abs(size.y) > MinimumCubeSize ? diameter / Mathf.Abs(size.y) : 1f,
            Mathf.Abs(size.z) > MinimumCubeSize ? diameter / Mathf.Abs(size.z) : 1f);
    }

    private bool IsSphere()
    {
        if (!hasResolvedSphere)
        {
            isSphere = ins.name.Contains(SpherePrefabName)
                || ins.GetComponentInChildren<SphereCollider>(true) != null;
            hasResolvedSphere = true;
        }

        return isSphere;
    }

    private float GetBasePitch()
    {
        return DynamicGlobalSettings.cameraMode == CameraMode.Overhead
            ? 90f
            : GetIsometricPitch();
    }

}
