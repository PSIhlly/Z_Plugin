using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryOverview;
using Z_String;
using System.Drawing;
using UnityEngine;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectMask
{

    public partial class UiModStoryMapObjectMaskParam
    {
        public MapMaskForm.Data data;
        public int id;
    }
    public partial class UiModStoryMapObjectMaskModel
    {
        public MapMaskForm.Data data;

        public int id;
    }
    public partial class UiModStoryMapObjectMaskCtrl
    {
        GameObject[] conditions;
        UiScrViewContainer<UiItemCtrl> con;
        public override void OnCreate()
        {
            conditions = new GameObject[6] {view.go_mask0, view.go_mask1, view.go_mask2, view.go_mask3, view.go_mask4, view.go_mask5 };
            con = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteMask(model.data.name);
            });
            view.ipt_label.onFinishInput += (s) =>
            {
                model.data.label = s;
            };
            view.ipt_name.onFinishInput += (s) =>
            {
                model.data.name = s;
            };
           
            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportMask(model.data.name, model.id);
            });
        }
        public override void OnShow()
        {
            model.id = -1;
            if (param != null)
            {
                model.id = param.id;
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.sta_show.ChangeState(model.id >= 0 ? 1 : 0);

            con.Clear();
            for (int i = 0; i < model.data.texsName.Count; i++)
            {
                con.Add(new UiItemParam()
                {
                    id = i
                });
            }
            con.Refresh();

            if (model.id >= 0)
            {
                view.img_image.sprite = StoryTexAssetForm.DataByName[model.data.texsName[model.id]].sprite;
                for(int i = 0;i< conditions.Length;i++)
                {
                    conditions[i].SetActive(i == model.id);
                }
            }
            
        }
    }

    public partial class UiItemParam
    {
        public int id;
    }
    public partial class UiItemModel
    {
        public int id;
    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                parent.model.data.texsName.Add("");
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.id = model.id;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.id = param.id;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_item.ChangeState(model.id != -1 && model.id == parent.model.id ? 1 : 0);
            view.img_.sprite = StoryTexAssetForm.DataByName[parent.model.data.texsName[model.id]].sprite;

        }
    }

}