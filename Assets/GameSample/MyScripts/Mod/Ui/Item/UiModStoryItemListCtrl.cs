using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem.Form;

namespace Ui.ModStory.ModStoryItem.ModStoryItemList
{

    public partial class UiModStoryItemListParam
    {

    }
    public partial class UiModStoryItemListModel
    {
        public string lab;
        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemListCtrl
    {

        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiBigItemCtrl> itemCon;
        public override void OnCreate()
        {

            labCon = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_labs);
            itemCon = new UiScrViewContainer<UiBigItemCtrl>(this, view.go_bigItem, view.scr_bigItems);

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {

            labCon.Clear();
            labCon.Add(new UiLabParam()
            {
                lab = null
            });
            foreach (var lab in ItemProductForm.DatasByLabel.Keys)
            {
                if(lab!="")
                labCon.Add(new UiLabParam()
                {
                    lab = lab
                });
            }
            labCon.Refresh();
            itemCon.Clear();
            var datas = model.lab == null || !ItemProductForm.DatasByLabelProtouid.ContainsKey((model.lab, 0)) ?
                (ItemProductForm.DatasByProtouid.ContainsKey(0)?(ItemProductForm.DatasByProtouid[0]):new List<ItemProductForm.Data>())
                : ItemProductForm.DatasByLabelProtouid[(model.lab, 0)];

            foreach (var data in datas.OrderBy(d => d.uid))
            {
                itemCon.Add(new UiBigItemParam()
                {
                    data = data
                });
            }
            itemCon.Add(new UiBigItemParam()
            {
                data = null
            });
            itemCon.Refresh();



        }
    }

    public partial class UiLabParam
    {
        public string lab;
    }
    public partial class UiLabModel
    {
        public string lab;
    }
    public partial class UiLabCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.model.lab = model.lab;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.lab = param.lab;
            Refresh();
        }
        public void Refresh()
        {

            view.sta_valid.ChangeState(model.lab == null ? 0 : 1);
            view.sta_.ChangeState(model.lab == parent.model.lab ? 1 : 0);
            if (model.lab != null)
            {
                view.txt_.text = model.lab;
            }
        }
    }


    public partial class UiBigItemParam
    {
        public ItemProductForm.Data data;
    }
    public partial class UiBigItemModel
    {
        public ItemProductForm.Data data;
    }
    public partial class UiBigItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateItem();
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelPage(1, model.data);
            });

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null)
            {
                view.txt_.text = model.data.name;
                view.img_.BindTexData(TexAssetForm.DataById[model.data.iconTexName]);
            }
        }
    }


}