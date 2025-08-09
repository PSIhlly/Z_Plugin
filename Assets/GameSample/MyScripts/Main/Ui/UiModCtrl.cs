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
using Z_DataSystem.Form;

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
            foreach (var data in StoryForm.DataById.Values)
            {
                con.Add(new UiStoryItemParam()
                {
                    data= data
                });
            }
            con.Add(new UiStoryItemParam()
            {
                data = null
            });
            con.Refresh();
        }

       

    }
    public partial class UiStoryItemParam
    {
        public StoryForm.Data data;
    }
    public partial class UiStoryItemModel
    {
        public StoryForm.Data data;
    }
    public partial class UiStoryItemCtrl
    {
       public override void OnCreate()
        {
            view.btn_mod.onClick.AddListener(() =>
            {
                if(model.data!=null)
                {
                    Main2StoryManager.instance.StartLoadStoryUgc(model.data.id);
                }else
                {
                    Main2StoryManager.instance.StartLoadStoryUgc(StoryForm.idChain.PeekId());
                }
                parent.Close();
            });
        }
        public override void OnShow()
        {
            model.data = param.data;
            
            Refresh();
        }
        public void Refresh()
        {
            view.sta_.ChangeState(model.data != null ? 1 : 0);
            if (model.data != null)
            { 
                view.txt_.text = model.data.name;
                view.img_.sprite = TexAssetForm.DataByName[model.data.icon].sprite;
            }
        }



    }
}
