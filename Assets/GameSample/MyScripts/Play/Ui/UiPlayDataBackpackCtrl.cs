using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem.Form;

namespace Ui.PlayData.PlayDataBackpack
{

    public partial class UiPlayDataBackpackParam
    {

    }
    public partial class UiPlayDataBackpackModel
    {
        public ItemProductForm.Data sel;
    }
    public partial class UiPlayDataBackpackCtrl
    {

        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {

            });
            view.btn_drop.onClick.AddListener(() =>
            {

            });
            view.btn_use.onClick.AddListener(() =>
            {

            });
            view.btn_equip.onClick.AddListener(() =>
            {

            });
            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_);

        }
        public override void OnShow()
        {
            model.sel = null;
            Refresh();
        }
        public void Refresh()
        {

            itemCon.Clear();
            for (int i = 0, icnt = PlayManager.instance.data.progress.bag.Count; i < icnt; i++)
            {
                itemCon.Add(new UiItemParam()
                {
                    data = ItemProductForm.DataByUid[PlayManager.instance.data.progress.bag[i]]
                }) ;
            }
            itemCon.Refresh();
            view.sta_show.ChangeState(model.sel != null?1:0);
            if (model.sel!=null)
            {
                view.img_.sprite = TexAssetForm.DataByName[model.sel.iconTexName].sprite;
                view.txt_amount.text = model.sel.amount.ToString();
                view.txt_desc.text = model.sel.desc;
            }
        }
        public void Sel(ItemProductForm.Data data)
        {
            model.sel = data;
            Refresh();
        }
    }


    public partial class UiItemParam
    {

        public ItemProductForm.Data data;
    }
    public partial class UiItemModel
    {

        public ItemProductForm.Data data;
    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.Sel(model.data);
            });

        }
        public override void OnShow()
        {
            if(param!=null)
            {
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.txt_amount.text = model.data.amount.ToString();
            view.img_.sprite = TexAssetForm.DataByName[model.data.iconTexName].sprite;
            view.sta_item.ChangeState(parent.model.sel == model.data ? 1 : 0);
        }
    }


}