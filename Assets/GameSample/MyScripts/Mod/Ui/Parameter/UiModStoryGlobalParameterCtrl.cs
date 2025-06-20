using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
namespace Ui.ModStory.ModStoryParameter.ModStoryGlobalParameter
{

    public partial class UiModStoryGlobalParameterParam
    {

    }
    public partial class UiModStoryGlobalParameterModel
    {

    }
    public partial class UiModStoryGlobalParameterCtrl
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
            foreach (var data in GlobalParamForm.DataByName.Values)
            {
                itemCon.Add(new UiItemParam()
                {
                    data= data
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
        public GlobalParamForm.Data data;
    }
    public partial class UiItemModel
    {
        public GlobalParamForm.Data data;
    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {
            
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateGlobalArg(null);
                parent.Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteGlobalArg(model.data.name);
                parent.Refresh();
            });
            view.ipt_value.onEndEdit.AddListener((s) =>
            {
                model.data.v = StringHelper.ToFloat(s,0f);
                Refresh();
            });
            view.ipt_name.onEndEdit.AddListener((s) =>
            {
                if(StringHelper.IsUniqueName(GlobalParamForm.DataByName.Keys, s))
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