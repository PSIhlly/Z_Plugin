using Z_Ui.Form.DialogForm;
using Z_Ui.Form.NpcForm;
using Z_Ui.Form.ImgForm;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Texture;
using Z_Ui.Dialog;
using UnityEngine.UI;
using Z_Ui;
using Z_Ui.Base;
using Ui.APanel;

public class Z_Ui_Sample : MonoBehaviour
{
    public Text t;
    private void Start()
    {
        
    }
    public void StartDialog()
    {
        var contentLst = new List<string>();
        var nameLst = new List<string>();
        var bgLst = new List<Sprite>();
        var avatarLst = new List<Sprite>();

        foreach (var v in DialogForm.Datas.Values)
        {
            if (v.groupId == 1)
            {
                contentLst.Add(v.text);
                nameLst.Add(NpcForm.Datas[v.speaker_npcId].name);
                bgLst.Add(TextureHelper.GetSpriteByPath(ImgForm.Datas[v.background_imgId].path));
                avatarLst.Add(TextureHelper.GetSpriteByPath(ImgForm.Datas[NpcForm.Datas[v.speaker_npcId].avatar_imgId].path));
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
      
    }


}

namespace Ui.APanel
{
    
    public partial class UiAPanelCtrl
    {
        UiScrViewContainer<UiJBCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiJBCtrl>(view.go_JB,view.scr_tt);
        }

        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();
            for (int i = 0; i < 30; i++)
                con.Add(new UiJBParam()
                {
                    id = i
                });
            con.Refresh();
        }

    }
    
    public partial class UiJBParam
    {
        public int id;
    }

    public partial class UiJBCtrl
    {
        public override void OnCreate()
        {
            view.btn_lj.onClick.AddListener(()=>
            {
                parent.Refresh();
            });
        }
        public override void OnShow()
        {
            view.txt_.text = param.id.ToString();
        }
    }

    }
