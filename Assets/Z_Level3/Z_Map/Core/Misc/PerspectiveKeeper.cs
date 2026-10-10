using UnityEngine;
using UnityEngine.Serialization;
using Z_Map;
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
    // Preserve the authored baseline if an already refreshed hierarchy is cloned.
    [SerializeField, HideInInspector]
    private bool hasImageBaseScale;
    [SerializeField, HideInInspector]
    private Vector3 imageBaseScale;
    private CameraMode lastCameraMode;
    [FormerlySerializedAs("scaleHolder")]
    public Transform rotateHolder;
    [SerializeField, HideInInspector]
    private Transform scaleCompensator;
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
                RefreshNow();
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
        if (!ResolveHierarchy())
            return;

        UpdateModel();
        insLastRot = ins.eulerAngles;
        insLastScale = ins.lossyScale;
        lastCameraMode = DynamicGlobalSettings.cameraMode;
        ApplyRotation();
        needsInitialUpdate = false;
    }

    public void LateUpdate()
    {
        if (!ResolveHierarchy())
            return;

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

    private bool ResolveHierarchy()
    {
        if (rotateHolder == null || rotateHolder.parent == null)
            return false;
        if (scaleCompensator == null)
        {
            // Model scale must be canceled BEFORE rotation. Keep this neutral
            // carrier outside rotateHolder and owned by the same pooled model;
            // create it only once, including for old/custom prefab hierarchies.
            ins = rotateHolder.parent;
            int siblingIndex = rotateHolder.GetSiblingIndex();
            scaleCompensator = new GameObject("imageScaleCompensator").transform;
            scaleCompensator.SetParent(ins, false);
            scaleCompensator.SetSiblingIndex(siblingIndex);
            rotateHolder.SetParent(scaleCompensator, false);
            rotateHolder.name = "rotateHolder";
        }
        ins = scaleCompensator.parent;
        return ins != null;
    }

    private void ApplyRotation()
    {
        float pitch = enableFixedXRotationDefault
            ? GetBasePitch()
            : rotateHolder.eulerAngles.x;
        float yaw = applyYRotationToLocalZ || enableFixedYRotation
            ? fixedYRotation
            : rotateHolder.eulerAngles.y;
        float roll = applyYRotationToLocalZ && !enableFixedZRotation0
            ? -ins.eulerAngles.y
            : 0f;

        // Compose the character/object direction as a rotation inside the
        // already tilted image plane. Mutating local Euler Z after setting a
        // world-space pitch mixes the carrier's Y rotation into the plane normal.
        Quaternion surfaceRotation = Quaternion.Euler(pitch, yaw, 0f);
        rotateHolder.rotation = surfaceRotation * Quaternion.AngleAxis(roll, Vector3.forward);
    }
    public void UpdateModel()
    {
        if (!ResolveHierarchy())
            return;
        if (!hasImageBaseScale)
        {
            imageBaseScale = transform.localScale;
            hasImageBaseScale = true;
        }
        // Refresh and camera switches must start from the authored image size,
        // not the diagonal stretch submitted by the previous owner/frame.
        transform.localScale = imageBaseScale;
        transform.localRotation = Quaternion.identity;
        rotateHolder.localScale = Vector3.one;
        switch (DynamicGlobalSettings.cameraMode)
        {
            case CameraMode.Overhead:
                scaleCompensator.localScale = Vector3.one;
                if (!IsSphere())
                    ApplyCubeRendererScale(false);
                rotateHolder.position = ins.position + Vector3.up * ins.localScale.y / 2 + Vector3.down * deepth;
                rotateHolder.eulerAngles = Vector3.right * 90;
                break;
            case CameraMode.Isometric:
                // Cubes use the Y-Z diagonal; spheres use a diameter-sized square.
                rotateHolder.eulerAngles = Vector3.right * GetIsometricPitch();
                if (IsSphere())
                    scaleCompensator.localScale = GetSphereRendererScale();
                else
                    ApplyCubeRendererScale(true);
                // Position after changing the neutral carrier's scale, otherwise
                // its depth offset would be scaled again along the model axes.
                rotateHolder.position = ins.position + new Vector3(0, 1, -1) * deepth;
                break;
        }
    }

    private void ApplyCubeRendererScale(bool isometric)
    {
        Vector3 cubeSize = ins.lossyScale;
        float height = Mathf.Abs(cubeSize.y);
        float depth = Mathf.Abs(cubeSize.z);
        // A parent's nonuniform scale acts AFTER a child's rotation and bends
        // its rendered axes. Neutralize that scale before orienting the image,
        // then put the final dimensions on img's own (rotated) local axes.
        // This changes visual transforms only; the sibling Collider is untouched.
        scaleCompensator.localScale = new Vector3(
            InverseDimension(cubeSize.x), InverseDimension(cubeSize.y), InverseDimension(cubeSize.z));
        float imageHeight = isometric ? Mathf.Sqrt(height * height + depth * depth) : depth;
        transform.localScale = Vector3.Scale(imageBaseScale,
            new Vector3(Mathf.Abs(cubeSize.x), imageHeight, 1f));
    }

    private static float InverseDimension(float size)
    {
        return Mathf.Abs(size) > MinimumCubeSize ? 1f / size : 1f;
    }

    private float GetIsometricPitch()
    {
        // Resizing affects img's local Y length, never the image-plane pitch.
        return SquareDiagonalPitch;
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
