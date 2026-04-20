using Form;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
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
    private GameObject effectPrefab => InstancePoolManager.instance.GetPrefab(MapInfo.imgName);
    private GameObject canvasPrefab => InstancePoolManager.instance.GetPrefab(MapInfo.canvasName);
    private Dictionary<int, CanvasHolder> canvasDic = new Dictionary<int, CanvasHolder>();
    private int updateFrame;
    private List<string> needShowParamName = new List<string>();
    private List<string> needShowParamNameWithoutPlayer = new List<string>();
    public PlaySceneEffectController(PlayManager super) : base(super)
    {
        this.Register();
    }

    public void Begin()
    {
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
        canvasDic.Clear(); updateFrame = 0;
    }
    public void CreatEffect(int uid, Vector3 pos, float rot, Action<ImageHolder> beforeUpdate = null)
    {
        var data = EffectForm.DataByUid[uid];
        var clips = data.clips;
        foreach (var eft in clips)
        {
            var img = InstancePoolManager.instance.CreateInstance(effectPrefab).GetComponentInChildren<ImageHolder>();

            if (GameManager.instance.curProgress.cameraMode == CameraMode.Overhead || data.ground)
                img.trs.eulerAngles = Vector3.right*90;
            else
                img.trs.eulerAngles = Vector3.right * 45;

            TimeManager.instance.CancelTimer(img.animTimer);
            int cur = -1;
            float startTime = Time.time;

            img.oriPos = pos;

            img.oriRot = rot;
            img.oriScale = Vector3.zero;

            img.trs.position = pos;
            img.trs.localEulerAngles = img.trs.localEulerAngles.NewSetZ(-rot);
            img.trs.localScale = Vector3.one;
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
                    propBlock.SetTexture("_Tex", TexAssetForm.DataByName[clip.tex].GetTex());


                    propBlock.SetFloat("_Alpha", clip.opacity);
                    img.render.SetPropertyBlock(propBlock);


                    beforeUpdate?.Invoke(img);
                    img.trs.position = img.oriPos + MapManager.instance.utilCtrl.MapPos2RealPos(new Vector3(clip.pos.x, clip.pos.y, clip.pos.z));

                    if (GameManager.instance.curProgress.cameraMode == CameraMode.Overhead || data.ground)
                        img.trs.eulerAngles = img.trs.localEulerAngles.NewSetY(img.oriRot + clip.rot);
                    else
                        img.trs.localEulerAngles = img.trs.localEulerAngles.NewSetZ(-(img.oriRot + clip.rot));
                    img.trs.localScale = clip.scale;

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

                    img.trs.localScale = Vector3.Lerp(clip.scale, clipNxt.scale, rate);

                }

                return false;
            }, img);
        }

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
                    pair.Value.Reset();
                    InstancePoolManager.instance.DeleteInstance(pair.Value.gameObject, canvasPrefab);

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
            canvasDic.Remove(unitData.uid);
            return;
        }
        var canvas = GetCanvas(unitData);
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
                canvas.ShowSlider(paramInfo[lst[i]].GetValue().num, paramInfo[lst[i]].GetMax().num, i);
            }

        }
    }
    public void FloatingText(string txt, Vector3 pos)
    {
        var o = InstancePoolManager.instance.CreateInstance(canvasPrefab);
        switch (GameManager.instance.curProgress.cameraMode)
        {
            case CameraMode.Overhead:

                o.transform.position = pos + Vector3.up * 1.4f;
                break;
            case CameraMode.Isometric:
                o.transform.position = pos + Vector3.up * 0.5f;
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
    public CanvasHolder GetCanvas(CharacterUnitForm.Data unitData)
    {
        if (!canvasDic.ContainsKey(unitData.uid))
        {
            canvasDic[unitData.uid] = InstancePoolManager.instance.CreateInstance(canvasPrefab).GetComponent<CanvasHolder>();
            canvasDic[unitData.uid].gameObject.SetActive(true);
        }
        return canvasDic[unitData.uid];
    }
    public void OnEvent(CharacterEvent evt)
    {
        if (!_super.enable)
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
