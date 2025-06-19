using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Ui.Start;
using Ui.EnterMain;
using Z_Text;

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
                UiManager.instance.ShowUi<UiEnterMainCtrl>();
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
            int i = 0;
            for (; i < 8; i++)
            {
                con.Add(new UiStoryItemParam()
                {
                    name="mod1",
                    id= i
                });
            }
            con.Add(new UiStoryItemParam()
            {
                name = null,
                id= i
            });
            con.Refresh();
        }

       

    }
    public partial class UiStoryItemParam
    {
        public string name;
        public int id;
    }
    public partial class UiStoryItemModel
    {
        public string name;
        public int id;
    }
    public partial class UiStoryItemCtrl
    {
       public override void OnCreate()
        {
            view.btn_mod.onClick.AddListener(() =>
            {
                if(model.name!=null)
                {

                    Main2StoryManager.instance.StartLoadStoryUgc(model.name);
                }else
                {
                    Main2StoryManager.instance.StartLoadStoryUgc("newStory"+ model.id);
                }
                parent.Close();
            });
        }
        public override void OnShow()
        {
            model.name = param.name;
            model.id = param.id;
            if(model.name==null)
            {
                TextManager.instance.GetTxt("new");
            }
            else
            {
                view.txt_modName.text = model.name;
            }
            Refresh();
        }
        public void Refresh()
        {
        }



    }
}
