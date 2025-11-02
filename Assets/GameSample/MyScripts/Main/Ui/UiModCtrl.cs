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
        UiScrViewContainer<UiItemCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);
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
            foreach (var data in StoryForm.DataById.Values)
            {
                con.Add(new UiItemParam()
                {
                    data = data
                });
            }
            con.Add(new UiItemParam()
            {
                data = null
            });
            con.Refresh();
        }



    }
    public partial class UiItemParam
    {
        public StoryForm.Data data;
    }
    public partial class UiItemModel
    {
        public StoryForm.Data data;
    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                Main2StoryManager.instance.StartLoadStoryUgc(StoryForm.idChain.PeekId());
                parent.Close();
            });
            view.btn_.onClick.AddListener(() =>
            {

                Main2StoryManager.instance.StartLoadStoryUgc(model.data.id);

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
            view.sta_exist.ChangeState(model.data != null ? 1 : 0);
            if (model.data != null)
            {
                view.txt_.text = model.data.name;
                view.img_.sprite = TexAssetForm.DataByName[model.data.icon].GetSprite();
            }
        }



    }
}
