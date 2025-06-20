using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
namespace Ui.ModStory.ModStoryParameter.ModStoryItemParameter
{

    public partial class UiModStoryItemParameterParam
    {

    }
    public partial class UiModStoryItemParameterModel
    {

    }
    public partial class UiModStoryItemParameterCtrl
    {

        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {

            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {

            itemCon.Clear();
            foreach (var data in ItemParamForm.DataByName.Values)
            {
                itemCon.Add(new UiItemParam()
                {
                    data = data
                });
            }
            itemCon.Add(new UiItemParam()
            {
                data = null
            });
            itemCon.Refresh();
        }
    }


    public partial class UiItemParam
    {
        public ItemParamForm.Data data;
    }
    public partial class UiItemModel
    {
        public ItemParamForm.Data data;
    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateItemArg(null);
                parent.Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteItemArg(model.data.name);
                parent.Refresh();
            });
            view.ipt_value.onEndEdit.AddListener((s) =>
            {
                model.data.v = StringHelper.ToFloat(s, 0f);
                Refresh();
            });
            view.ipt_name.onEndEdit.AddListener((s) =>
            {
                if (StringHelper.IsUniqueName(ItemParamForm.DataByName.Keys, s))
                {
                    model.data.name = s;
                }
                Refresh();
            });

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {
            view.sta_item.ChangeState(model.data == null ? 0 : 1);

            if (model.data != null)
            {
                view.ipt_name.Set(model.data.name);
                view.ipt_value.Set(model.data.v.ToString("0.##"));
            }

        }
    }


}