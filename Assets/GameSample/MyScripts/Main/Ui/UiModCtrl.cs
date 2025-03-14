using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Ui.Start;
using Ui.ModStory;

namespace Ui.Mod
{
    public partial class UiModCtrl
    {
        UiScrViewContainer<UiStoryItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiStoryItemCtrl>(view.go_storyItem, view.scr_tt);
            view.btn_back.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiStartCtrl>();
                Close();
            });
        }

        
        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();
            for (int i = 0; i < 1; i++)
            {
                con.Add(new UiStoryItemParam()
                {
                    name="mod1"
                });
            }
            con.Refresh();
        }

       

    }
    public partial class UiStoryItemParam
    {
        public string name;
    }
    public partial class UiStoryItemCtrl
    {
       public override void OnCreate()
        {
            view.btn_mod.onClick.AddListener(() =>
            {
                Main2StoryManager.instance.StartLoadStoryUgc("story1");
                UiManager.instance.ShowUi<UiModStoryCtrl>();
                parent.Close();
            });
        }
        public override void OnShow()
        {
            view.txt_modName.text = param.name;
            Refresh();
        }
        public void Refresh()
        {
        }



    }
}
