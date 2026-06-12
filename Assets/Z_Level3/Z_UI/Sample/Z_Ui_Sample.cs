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
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Ui.Form.Sample_DialogForm;
using Z_Ui.Form.Sample_NpcForm;
using Z_Ui.Form.Sample_ImgForm;

public class Z_Ui_Sample : MonoBehaviour
{
    public Text t;
    public Texture[] bg;
    public void StartDialog()
    {
        var contentLst = new List<string>();
        var nameLst = new List<string>();
        var bgLst = new List<int>();
        var videoLst = new List<int>();
        var audioLst = new List<int>();
        var avatarLst = new List<int>();

        foreach (var v in Sample_DialogForm.Datas.Values)
        {
            if (v.groupId == 1)
            {
                contentLst.Add(v.text);
                nameLst.Add(Sample_NpcForm.Datas[v.speaker_npcId].name);

                var bgForm = Sample_ImgForm.Datas[v.background_imgId];
                var bgTexData = AssetManager.instance.texCtrl.CreateDataByPath(Application.dataPath + bgForm.path, bgForm.id + "bg");
                TexAssetForm.AddData(bgTexData);
                bgLst.Add(bgTexData.id);

                videoLst.Add(0);
                audioLst.Add(0);

                var avatarForm = Sample_ImgForm.Datas[Sample_NpcForm.Datas[v.speaker_npcId].avatar_imgId];
                var avatarTexData = AssetManager.instance.texCtrl.CreateDataByPath(Application.dataPath + avatarForm.path, avatarForm.id + "avt");
                TexAssetForm.AddData(avatarTexData);
                avatarLst.Add(avatarTexData.id);
            }
        }
        DialogManager.instance.Begin(nameLst, contentLst, bgLst, videoLst, avatarLst, audioLst, OnComplete);
    }
    private void OnComplete()
    {
        Debug.Log("End");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            NotifyManager.instance.AddTip("tips"+UnityEngine.Random.Range(1,99));
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            NotifyManager.instance.AddPopup("tips", "tips", false, new List<string>() { "ok", "cancel" }, new List<Func<bool>>() { () => { Debug.Log("ok"); return false; }, () => { Debug.Log("close"); return true; } });
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartDialog();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            var items = new EntryItem();
            items.Add("a");
            items.subs["a"].Add("b");
            items.subs["a"].subs["b"].Add("bb");
            items.subs["a"].subs["b"].Add("cc");
            items.Add("c");
            items.subs["c"].Add("d");

            NotifyManager.instance.AddMultipleChoose("Ch", false, (item) =>
            {
                return true;
            }, items);

        }
    }


}

