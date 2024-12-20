using Z_Ui.Form.Sample_DialogForm;
using Z_Ui.Form.Sample_NpcForm;
using Z_Ui.Form.Sample_ImgForm;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Texture;
using Z_Ui.Dialog;
using UnityEngine.UI;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;
using System;
using Z_Time;

public class Z_Ui_Sample : MonoBehaviour
{
    public Text t;
    public void StartDialog()
    {
        var contentLst = new List<string>();
        var nameLst = new List<string>();
        var bgLst = new List<Sprite>();
        var avatarLst = new List<Sprite>();

        foreach (var v in Sample_DialogForm.Datas.Values)
        {
            if (v.groupId == 1)
            {
                contentLst.Add(v.text);
                nameLst.Add(Sample_NpcForm.Datas[v.speaker_npcId].name);
                bgLst.Add(TextureHelper.GetSpriteByPath(Application.dataPath+Sample_ImgForm.Datas[v.background_imgId].path));
                avatarLst.Add(TextureHelper.GetSpriteByPath(Application.dataPath + Sample_ImgForm.Datas[Sample_NpcForm.Datas[v.speaker_npcId].avatar_imgId].path));
            }
        }
        DialogManager.instance.Begin(nameLst, contentLst, bgLst, avatarLst, OnComplete);
    }
    private void OnComplete()
    {
        Debug.Log("End");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            NotifyManager.instance.AddTip("tips");
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            NotifyManager.instance.AddPopup("tips",false,new List<string>() { "ok", "cancel" }, new List<Func<bool>>() { ()=> { Debug.Log("ok"); return false; }, () => { Debug.Log("close"); return true; } });
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartDialog();
        }
    }


}

