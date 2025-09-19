using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
using Z_DataSystem.Form;
using Z_DataSystem;

namespace Ui.ModStory.ModStoryItem.ModStoryItemUnit.ModStoryItemUnitOverview
{

    public partial class UiModStoryItemUnitOverviewParam
    {
        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemUnitOverviewModel
    {
        public ItemProductForm.Data data;

    }
    public partial class UiModStoryItemUnitOverviewCtrl:IZ_Listener<AssetEvent>
    {

        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteItem(model.data.uid);
                parent.parent.SelPage(0);

            });
            view.ipt_name.onFinishInput+=(s)=>
            {
                var lst = new List<string>();
                foreach(var data in ItemProductForm.DataByName.Values)
                    lst.Add(data.name);

                if(StringHelper.IsUniqueName(lst, s))
                    model.data.name = s;
            };
            view.ipt_label.onFinishInput+=(s)=>
            {
                model.data.label = s;
            };
            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportItemIcon(model.data.uid);
            });
            view.ipt_desc.onFinishInput += (s) =>
            {
                model.data.desc = s;
                Refresh();
            };

        }

        public void OnEvent(AssetEvent evt)
        {
            if (active)
                Refresh();
        }

        public override void OnShow()
        {
            if (param != null)
                model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.ipt_name.Set(model.data.name);
            view.ipt_label.Set(model.data.label);
            view.img_image.sprite = TexAssetForm.DataByName[model.data.iconTexName].sprite;
            view.ipt_desc.Set(model.data.desc);
        }
    }

}