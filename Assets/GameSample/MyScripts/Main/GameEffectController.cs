using Form;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem;
using Z_DesignStyle;
using Z_Map;
using Z_UnitSystem;
using Z_Debug;
using Z_Code.Form;
using Z_Ui.Notify;
using Z_Text;
using System;
using Z_ByteSerialize;
using Z_Code;
using Unity.VisualScripting;
using Z_DataSystem.Form;
using Z_Time;
using Z_Math;


public class GameEffectController : Z_Controller<GameManager>
{
    private GameObject prefab => InstancePoolManager.instance.GetPrefab(GlobalNameHelper.GetInternalPrefabName(MapData.imgName));

    public GameEffectController(GameManager super) : base(super)
    {

    }


   public void CreatEffect(int uid,Vector3 pos, float rot)
    {
        var img = InstancePoolManager.instance.CreateInstance(prefab).GetComponentInChildren<ImageHolder>();

        TimeManager.instance.CancelTimer(img.animTimer);
        int cur = 0;
        float startTime = Time.time;

        img.oriPos = pos;
        img.oriRot = rot;
        img.oriScale = Vector3.one;

        img.trs.position = pos;
        img.trs.eulerAngles = img.trs.localEulerAngles.NewSetY(rot);
        img.trs.localScale = Vector3.one;

        img.animTimer = TimeManager.instance.StartTimer(0,0.02f, () =>
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            float progress = Time.time - startTime;
            var clips = EffectForm.DataByUid[uid].clips;

            var clip = clips[cur];
            if (progress > clips[cur].time)
            {
                cur++;
                if (clips.Count > cur)
                {
                    InstancePoolManager.instance.DeleteInstance(img.gameObject, prefab);
                    return true;
                }

                clip = clips[cur];
                img.render.GetPropertyBlock(propBlock);
                propBlock.SetTexture("_Tex", TexAssetForm.DataByName[clip.tex].tex);

                img.trs.position = img.oriPos+clip.pos;
                img.trs.eulerAngles = img.trs.localEulerAngles.NewSetY(img.oriRot+ clip.rot);
                img.trs.localScale = img.oriScale+ clip.scale;
                img.trs.localScale = img.oriScale+ clip.scale;
                propBlock.SetFloat("_Alpha",clip.opacity);
                img.render.SetPropertyBlock(propBlock);
            }
            else if (clip.transition&& clips.Count > cur+1)
            {
                img.render.GetPropertyBlock(propBlock);
                var clipNxt = clips[cur + 1];
                float rate = Mathf.Min(1,progress / clips[cur].time);
                img.trs.position = img.oriPos + Vector3.Lerp(clip.pos, clipNxt.pos,rate);
                img.trs.eulerAngles = img.trs.localEulerAngles.NewSetY(img.oriRot + (clip.rot + (clipNxt.rot - clip.rot) * rate));
                propBlock.SetFloat("_Alpha", clip.opacity + (clipNxt.opacity - clip.opacity) * rate);
                img.render.SetPropertyBlock(propBlock);
            }

            return false;
        }, img);
       
    }

}
