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
using Z_DataSystem;
using Z_DataSystem.Form;

public class Z_Ui_Sample : MonoBehaviour
{
    public Text t;
    public void StartDialog()
    {
        var contentLst = new List<string>();
        var nameLst = new List<string>();
        var bgLst = new List<string>();
        var avatarLst = new List<string>();

        foreach (var v in Sample_DialogForm.Datas.Values)
        {
            if (v.groupId == 1)
            {
                contentLst.Add(v.text);
                nameLst.Add(Sample_NpcForm.Datas[v.speaker_npcId].name);
                var bgForm = Sample_ImgForm.Datas[v.background_imgId];
                TexAssetForm.AddData(AssetManager.instance.LoadTexPath(Application.dataPath + bgForm.path, bgForm.id + "bg"));
                bgLst.Add(bgForm.id + "bg");

                var avatarForm = Sample_ImgForm.Datas[Sample_NpcForm.Datas[v.speaker_npcId].avatar_imgId];
                TexAssetForm.AddData(AssetManager.instance.LoadTexPath(Application.dataPath + avatarForm.path, avatarForm.id + "avt"));
                avatarLst.Add(avatarForm.id + "avt");
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
            NotifyManager.instance.AddPopup("tips", "tips", false, new List<string>() { "ok", "cancel" }, new List<Func<bool>>() { () => { Debug.Log("ok"); return false; }, () => { Debug.Log("close"); return true; } });
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartDialog();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            var sub = new Dictionary<string, (Sprite,object)>()
            {
                { "AAAA", (null,new Dictionary<string, (Sprite,object)>(){
                    { "AAA", (null,new Dictionary<string, (Sprite,object)>()
                        {
                        { "AA",(null,null)},
                        { "BB",(null,null) },
                        }) },
                    { "AAB", (null,new Dictionary<string, (Sprite,object)>()
                        {
                        { "AB",(null,null) },
                        { "BC",(null,null)},
                        }) }
                } 
                )},
                { "BBBB",(null, new Dictionary<string, (Sprite,object)>(){
                    { "BBB",(null, new Dictionary<string, (Sprite,object)>()
                        {
                        { "DD",(null,null) },
                            { "EE",(null,null) },
                        })},
                    { "BBC", (null,new Dictionary<string, (Sprite,object)>()
                        {
                        { "FF",(null,null)},
                        { "GG",(null,null)},
                        })}
                }
                ) }
            };
           
            NotifyManager.instance.AddMultipleChoose("Ch", false, 2, (lst) =>
            {
                return true;
            }, sub);

        }
    }


}

