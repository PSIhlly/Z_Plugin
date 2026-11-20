using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;
using Z_ByteSerialize;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Math;
using Z_Text;
using Z_Time;
using Z_Ui.Base;
using Z_Ui.Notify;
using Z_UnitSystem;


public class PlaySceneEffectController : Z_Controller<PlayManager>, IZ_Listener<CharacterEvent>
{
    // 侧视(Isometric)下非地面特效的 X 轴倾角回退值：贴图 Y 轴跨过 clip 的
    // 高/深对角线（公式与 PerspectiveKeeper 的立方体一致），只有高和深都为 0
    // 时才用它；此时 45° 也正好是正对相机。
    private const float IsometricVerticalEffectPitch = 45f;
    private GameObject effectPrefab => InstancePoolManager.instance.GetPrefab(MapInfo.GetPrefabName("img"));
    private GameObject canvasPrefab => InstancePoolManager.instance.GetPrefab(MapInfo.GetPrefabName("canvas"));
    private Dictionary<int, CanvasHolder> canvasDic = new Dictionary<int, CanvasHolder>();
    private readonly Dictionary<CanvasHolder, int> floatingCanvases = new Dictionary<CanvasHolder, int>();
    private int updateFrame;
    private int lifecycleVersion;
    private bool active;
    private List<string> needShowParamName = new List<string>();
    private List<string> needShowParamNameWithoutPlayer = new List<string>();
    public PlaySceneEffectController(PlayManager super) : base(super)
    {
        this.Register();
    }

    public void Begin()
    {
        ReleaseAllCanvases();
        lifecycleVersion++;
        active = true;
        needShowParamName.Clear();
        needShowParamNameWithoutPlayer.Clear();
        foreach (var prm in CharacterParamForm.DataByName.Values)
        {

            var type = prm.showType;
            if (type == ParamShowType.AlwaysWithPanelAndScene)
            {
                needShowParamName.Add(prm.name);
                needShowParamNameWithoutPlayer.Add(prm.name);
            }
            else if (type == ParamShowType.AlwaysWithPanelAndSceneWithoutPlayer)
            {
                needShowParamNameWithoutPlayer.Add(prm.name);
            }
        }
        updateFrame = 0;
    }

    public void End()
    {
        active = false;
        lifecycleVersion++;
        ReleaseAllCanvases();
    }

    private void ReleaseAllCanvases()
    {
        foreach (var canvas in canvasDic.Values)
            ReleaseCanvas(canvas);
        canvasDic.Clear();
        foreach (var pair in floatingCanvases)
        {
            // An externally recycled Canvas may already belong to a new user.
            if (pair.Key != null && pair.Key.LifecycleVersion == pair.Value)
                ReleaseCanvas(pair.Key);
        }
        floatingCanvases.Clear();
    }

    private void ReleaseCanvas(CanvasHolder canvas)
    {
        if (canvas == null)
            return;

        canvas.Reset();
        var prefab = canvasPrefab;
        if (prefab != null)
            InstancePoolManager.instance.DeleteInstance(canvas.gameObject, prefab);
        else
            UnityEngine.Object.Destroy(canvas.gameObject);
    }
    public void CreatEffect(int uid, Vector3 pos, float rot, Action<ImageHolder> beforeUpdate = null)
    {
        var data = EffectForm.DataByUid[uid];
        var clips = data.clips;
        foreach (var eft in clips)
        {
            var img = InstancePoolManager.instance.CreateInstance(effectPrefab).GetComponentInChildren<ImageHolder>();
            // 不再区分 ground：贴图倾角由 clip 的高/深对角线决定，高=0 时 atan2 正好是
            // 90°（平铺在地面上），这就是原来的地面特效。
            bool isIsometric = GameManager.instance.curProgress.cameraMode == CameraMode.Isometric;

            if (isIsometric)
                img.trs.rotation = GetIsometricRotation(GetIsometricPitch(Vector3.one), -rot);
            else
                img.trs.eulerAngles = Vector3.right * 90;

            TimeManager.instance.CancelTimer(img.animTimer);
            int cur = -1;
            float startTime = Time.time;

            img.oriPos = pos;

            img.oriRot = rot;
            img.oriScale = Vector3.zero;

            img.trs.position = pos;
            if (!isIsometric)
                img.trs.localEulerAngles = img.trs.localEulerAngles.NewSetZ(-rot);
            img.trs.localScale = GetEffectScale(Vector3.one, isIsometric);
            var propBlock = new MaterialPropertyBlock();
            Texture currentTexture = null;
            bool animatedTexture = false;
            Func<bool> updateEffect = () =>
            {
                float progress = Time.time - startTime;
                var clip = cur == -1 ? null : eft[cur];
                if (cur == -1 || progress > eft[cur].time)
                {
                    startTime = Time.time;
                    cur++;
                    if (eft.Count <= cur)
                    {
                        InstancePoolManager.instance.DeleteInstance(img.gameObject, effectPrefab);
                        return true;
                    }

                    clip = eft[cur];
                    progress = 0;
                    img.render.GetPropertyBlock(propBlock);
                    propBlock.SetFloat("_Alpha", clip.opacity);
                    img.render.SetPropertyBlock(propBlock);
                    animatedTexture = TexAssetForm.DataById[clip.tex].GetAnimationDuration() > 0;
                    currentTexture = TextureAnimation.GetSample(clip.tex, 0, 0).texture;
                    DisFadeRenderRouting.SetTexture(img.render, currentTexture, GetEffectRoute(clip.opacity));


                    beforeUpdate?.Invoke(img);
                    img.trs.position = img.oriPos + clip.pos;

                    if (isIsometric)
                        img.trs.rotation = GetIsometricRotation(
                            GetIsometricPitch(clip.scale), -(img.oriRot + clip.rot));
                    else
                        img.trs.eulerAngles = img.trs.localEulerAngles.NewSetY(img.oriRot + clip.rot);
                    img.trs.localScale = GetEffectScale(clip.scale, isIsometric);

                }
                else if (clip.transition && eft.Count > cur + 1)
                {
                    img.render.GetPropertyBlock(propBlock);
                    var clipNxt = eft[cur + 1];
                    float rate = Mathf.Min(1, progress / eft[cur].time);
                    propBlock.SetFloat("_Alpha", clip.opacity + (clipNxt.opacity - clip.opacity) * rate);
                    DisFadeRenderRouting.Apply(img.render, propBlock, GetEffectRoute(propBlock.GetFloat("_Alpha")));

                    beforeUpdate?.Invoke(img);
                    img.trs.position = img.oriPos + Vector3.Lerp(clip.pos, clipNxt.pos, rate);

                    var lerpScale = Vector3.Lerp(clip.scale, clipNxt.scale, rate);
                    if (isIsometric)
                        img.trs.rotation = GetIsometricRotation(
                            GetIsometricPitch(lerpScale),
                            -(img.oriRot + (clip.rot + (clipNxt.rot - clip.rot) * rate)));
                    else
                        img.trs.eulerAngles = img.trs.localEulerAngles.NewSetY(img.oriRot + (clip.rot + (clipNxt.rot - clip.rot) * rate));

                    img.trs.localScale = GetEffectScale(lerpScale, isIsometric);

                }

                if (animatedTexture)
                {
                    var texture = TextureAnimation.GetSample(clip.tex, 0, progress).texture;
                    if (texture != currentTexture)
                    {
                        currentTexture = texture;
                        DisFadeRenderRouting.SetTexture(img.render, texture, GetEffectRoute(propBlock.GetFloat("_Alpha")));
                    }
                }
                return false;
            };
            // Show frame zero synchronously, then sample variable GIF/WebP delays
            // once per rendered frame. Bind the timer to this pooled image.
            if (!updateEffect())
                img.animTimer = TimeManager.instance.StartFrameTimer(updateEffect, img);
        }

    }

    private static DisFadeRenderRouting.Route GetEffectRoute(float opacity)
    {
        return opacity <= 0f ? DisFadeRenderRouting.Route.Hidden
            : opacity < 1f ? DisFadeRenderRouting.Route.Transparent : DisFadeRenderRouting.Route.Opaque;
    }

    private static Vector3 GetEffectScale(Vector3 scale, bool isIsometric)
    {
        // 与 PerspectiveKeeper 的立方体贴面对齐：clip 的 y 是高、z 是深。
        // 侧视时贴图 Y 轴跨过 Y-Z 对角线；俯视时相机垂直向下，只跨深度。
        // 高=0 时侧视长度退化成深度、倾角 90°，也就是原来的地面特效。
        float height = Mathf.Abs(scale.y);
        float depth = Mathf.Abs(scale.z);
        float length = isIsometric ? Mathf.Sqrt(height * height + depth * depth) : depth;
        return new Vector3(scale.x, length, 1f);
    }

    private static float GetIsometricPitch(Vector3 scale)
    {
        float height = Mathf.Abs(scale.y);
        float depth = Mathf.Abs(scale.z);
        return height == 0f && depth == 0f
            ? IsometricVerticalEffectPitch
            : Mathf.Atan2(depth, height) * Mathf.Rad2Deg;
    }

    private static Quaternion GetIsometricRotation(float pitch, float roll)
    {
        // 先立起对角线倾角，再在贴图平面内做方向 roll；不要改局部欧拉 Z，
        // 否则父级的 Y 旋转会混进平面法线。
        return Quaternion.Euler(pitch, 0f, 0f) * Quaternion.AngleAxis(roll, Vector3.forward);
    }

    public void Update()
    {
        if (updateFrame < Time.frameCount)
        {
            updateFrame = Time.frameCount + 10;
            var temps = new List<int>();
            foreach (var pair in canvasDic)
            {
                if (!CharacterUnitForm.DataByUid.ContainsKey(pair.Key))
                {
                    temps.Add(pair.Key);
                    ReleaseCanvas(pair.Value);
                }
            }
            foreach (var temp in temps)
            {
                canvasDic.Remove(temp);
            }
        }
    }

    public void BindCanvas(CharacterUnitForm.Data unitData)
    {

        var productData = CharacterProductForm.DataByUid.GetDv(unitData.unit.productInfo.Item1, null);
        if (productData == null)
        {
            if (canvasDic.TryGetValue(unitData.uid, out var oldCanvas))
            {
                ReleaseCanvas(oldCanvas);
                canvasDic.Remove(unitData.uid);
            }
            return;
        }
        int version = lifecycleVersion;
        TimeManager.instance.AddCurLateUpdateWithoutCheckAction(() =>
        {
            if (!active || version != lifecycleVersion
                || !CharacterUnitForm.DataByUid.TryGetValue(unitData.uid, out var currentData)
                || !ReferenceEquals(currentData, unitData))
                return;

            var canvas = GetCanvas(unitData);
            if (canvas == null || GameManager.instance.curProgress == null)
                return;
            switch (GameManager.instance.curProgress.cameraMode)
            {
                case CameraMode.Overhead:
                    canvas.transform.position = unitData.unit.data.pos + Vector3.up * 1.3f + Vector3.forward * 0.5f;
                    canvas.transform.eulerAngles = Vector3.right * 90;
                    break;
                case CameraMode.Isometric:
                    canvas.transform.position = unitData.unit.data.pos + Vector3.up * 1.3f;
                    canvas.transform.eulerAngles = Vector3.right * 45;
                    break;
            }
            canvas.SetDirection(unitData.pos, unitData.euler.y);
            var paramInfo = productData.paramDic;
            var lst = unitData.unit.productInfo.Item1 == GameManager.instance.curProgress.characterUid ? needShowParamName : needShowParamNameWithoutPlayer;

            for (int i = 0, icnt = lst.Count; i < icnt; i++)
            {
                if (paramInfo.ContainsKey(lst[i]))
                {
                    var prm = paramInfo[lst[i]];
                    var maxPrm = string.IsNullOrEmpty(prm.max) ? null : paramInfo.GetDv(prm.max, null);
                    float max = maxPrm == null ? GlobalSettings.MAX : maxPrm.GetValue().num;
                    var minPrm = string.IsNullOrEmpty(prm.min) ? null : paramInfo.GetDv(prm.min, null);
                    float min = minPrm == null ? 0 : minPrm.GetValue().num;

                    canvas.ShowSlider(prm.GetValue().num - min, max - min, i);
                }

            }
        });

    }
    public void FloatingText(string txt, Vector3 pos)
    {
        if (!active || GameManager.instance.curProgress == null)
            return;
        var o = InstancePoolManager.instance.CreateInstance(canvasPrefab);
        var canvas = o.GetComponent<CanvasHolder>();
        canvas.Reset();
        switch (GameManager.instance.curProgress.cameraMode)
        {
            case CameraMode.Overhead:

                o.transform.eulerAngles = Vector3.right * 90;
                o.transform.position = pos + Vector3.up * 1.4f;
                break;
            case CameraMode.Isometric:
                o.transform.position = pos + Vector3.up * 0.5f;
                o.transform.eulerAngles = Vector3.right * 45;
                break;
        }
        o.SetActive(true);
        int version = canvas.LifecycleVersion;
        floatingCanvases[canvas] = version;
        canvas.ToastText(txt, 2, () =>
        {
            if (!floatingCanvases.TryGetValue(canvas, out var currentVersion) || currentVersion != version)
                return;
            floatingCanvases.Remove(canvas);
            if (canvas != null && canvas.LifecycleVersion == version)
                ReleaseCanvas(canvas);
        });
    }

    public void ChatText(int characterUid, string txt, int img, float time)
    {
        var productData = CharacterProductForm.DataByUid.GetDv(characterUid, null);
        var unit = _super.sceneCtrl.GetCharacterUnit(characterUid);
        if (productData != null)
            ChatText(unit, txt, img, time);
    }

    public void ChatText(CharacterUnitForm.Data unit, string txt, int img, float time)
    {
        if (unit?.unit == null || !Z_Code.SceneUnitCommand.IsLive(unit.unit, unit)
            || !canvasDic.TryGetValue(unit.uid, out var canvas))
            return;
        if (canvas != null)
            canvas.Chat(txt, img, time);
    }
    public CanvasHolder GetCanvas(CharacterUnitForm.Data unitData)
    {
        if (!active)
            return null;

        if (!canvasDic.TryGetValue(unitData.uid, out var canvas) || canvas == null)
        {
            var prefab = canvasPrefab;
            if (prefab == null)
                return null;
            canvas = InstancePoolManager.instance.CreateInstance(prefab).GetComponent<CanvasHolder>();
            canvasDic[unitData.uid] = canvas;
            canvas.gameObject.SetActive(true);
        }
        return canvas;
    }
    public void OnEvent(CharacterEvent evt)
    {
        if (evt.type == MapEventType.Remove)
        {
            if (canvasDic.TryGetValue(evt.unit.data.uid, out var canvas))
            {
                canvasDic.Remove(evt.unit.data.uid);
                ReleaseCanvas(canvas);
            }
            return;
        }
        if (!_super.enable || !active)
        {
            return;
        }
        switch (evt.type)
        {
            case MapEventType.Show:
                break;
            case MapEventType.AfterUpdate:
                //manage nav
                if (evt.unit.isVising)
                {
                    BindCanvas(evt.unit.data);
                }
                break;
        }
    }
}
