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
    private const float IsometricVerticalEffectHeightScale = 1.41421356f;
    // 侧视(Isometric)下非地面特效的 X 轴倾角：相机俯角本身就是 45°，
    // 0° 表示贴图平面竖直立在地面上，45° 表示正对相机。
    private const float IsometricVerticalEffectPitch = 45f;
    private GameObject effectPrefab => InstancePoolManager.instance.GetPrefab(MapInfo.GetPrefabName("img"));
    private GameObject canvasPrefab => InstancePoolManager.instance.GetPrefab(MapInfo.GetPrefabName("canvas"));
    private Dictionary<int, CanvasHolder> canvasDic = new Dictionary<int, CanvasHolder>();
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
            bool isIsometricVerticalEffect = GameManager.instance.curProgress.cameraMode == CameraMode.Isometric && !data.ground;

            if (isIsometricVerticalEffect)
                img.trs.eulerAngles = Vector3.right * IsometricVerticalEffectPitch;
            else
                img.trs.eulerAngles = Vector3.right * 90;

            TimeManager.instance.CancelTimer(img.animTimer);
            int cur = -1;
            float startTime = Time.time;

            img.oriPos = pos;

            img.oriRot = rot;
            img.oriScale = Vector3.zero;

            img.trs.position = pos;
            img.trs.localEulerAngles = img.trs.localEulerAngles.NewSetZ(-rot);
            img.trs.localScale = GetEffectScale(Vector3.one, isIsometricVerticalEffect);
            img.animTimer = TimeManager.instance.StartTimerImmediate(0, 0.0001f, () =>
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
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
                    img.render.GetPropertyBlock(propBlock);
                    propBlock.SetTexture("_Tex", TexAssetForm.DataById[clip.tex].GetTex());

                    propBlock.SetFloat("_Alpha", clip.opacity);
                    img.render.SetPropertyBlock(propBlock);


                    beforeUpdate?.Invoke(img);
                    img.trs.position = img.oriPos + MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(clip.pos.x, clip.pos.y, clip.pos.z));

                    if (GameManager.instance.curProgress.cameraMode == CameraMode.Overhead || data.ground)
                        img.trs.eulerAngles = img.trs.localEulerAngles.NewSetY(img.oriRot + clip.rot);
                    else
                        img.trs.localEulerAngles = img.trs.localEulerAngles.NewSetZ(-(img.oriRot + clip.rot));
                    img.trs.localScale = GetEffectScale(clip.scale, isIsometricVerticalEffect);

                }
                else if (clip.transition && eft.Count > cur + 1)
                {
                    img.render.GetPropertyBlock(propBlock);
                    var clipNxt = eft[cur + 1];
                    float rate = Mathf.Min(1, progress / eft[cur].time);
                    propBlock.SetFloat("_Alpha", clip.opacity + (clipNxt.opacity - clip.opacity) * rate);
                    img.render.SetPropertyBlock(propBlock);

                    beforeUpdate?.Invoke(img);
                    img.trs.position = img.oriPos + Vector3.Lerp(clip.pos, clipNxt.pos, rate);

                    if (GameManager.instance.curProgress.cameraMode == CameraMode.Overhead || data.ground) img.trs.eulerAngles = img.trs.localEulerAngles.NewSetY(img.oriRot + (clip.rot + (clipNxt.rot - clip.rot) * rate));
                    else
                        img.trs.localEulerAngles = img.trs.localEulerAngles.NewSetZ(-(img.oriRot + (clip.rot + (clipNxt.rot - clip.rot) * rate)));

                    img.trs.localScale = GetEffectScale(Vector3.Lerp(clip.scale, clipNxt.scale, rate), isIsometricVerticalEffect);

                }

                return false;
            }, img);
        }

    }

    private static Vector3 GetEffectScale(Vector3 scale, bool isIsometricVerticalEffect)
    {
        // √2 高度补偿只针对 X 轴 0°（竖直立在地面上）的贴图平面：
        // 45° 俯视相机下它的纵向会被压缩 cos45。改成 45° 正对相机后不再压缩，
        // 所以这里跟随 IsometricVerticalEffectPitch，避免出现 1.414 倍的纵向拉伸。
        if (isIsometricVerticalEffect && Mathf.Approximately(IsometricVerticalEffectPitch, 0f))
            scale.y *= IsometricVerticalEffectHeightScale;

        return scale;
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
                    canvas.transform.eulerAngles = Vector3.zero;
                    break;
                case CameraMode.Isometric:
                    canvas.transform.position = unitData.unit.data.pos + Vector3.up * 1.3f;
                    canvas.transform.eulerAngles = Vector3.right * 45;
                    break;
            }
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
        var o = InstancePoolManager.instance.CreateInstance(canvasPrefab);
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
        o.GetComponent<CanvasHolder>().ToastText(txt, 2);
        TimeManager.instance.StartTimer(3, 0, () =>
        {
            InstancePoolManager.instance.DeleteInstance(o, canvasPrefab);
            return true;
        }, o.GetComponent<CanvasHolder>());
    }

    public void ChatText(int characterUid, string txt, int img, float time)
    {
        var productData = CharacterProductForm.DataByUid.GetDv(characterUid, null);
        var unit = _super.sceneCtrl.GetCharacterUnit(characterUid);
        if (productData == null || unit == null || !canvasDic.ContainsKey(unit.uid))
        {
            return;
        }
        var canvas = canvasDic[unit.uid];
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
