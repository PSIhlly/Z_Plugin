using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_DataSystem.Form;

namespace Ui.ModStoryItemListArguments.ModStoryItemListArgumentsCustom
{
    public partial class UiModStoryItemListArgumentsCustomParam
    {
        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemListArgumentsCustomModel
    {
        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemListArgumentsCustomCtrl
    {

        UiScrViewContainer<UiUnitCtrl> con;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiUnitCtrl>(view.go_unit,view.scr_units);
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();
            foreach(var data in ItemParamForm.DataByUid.Values)
            {
                con.Add(new UiUnitParam()
                {
                    data = data
                });
            }
            con.Refresh();
        }
    }
    public partial class UiUnitModel
    {
        public ItemParamForm.Data data;
    }
    public partial class UiUnitParam
    {
        public ItemParamForm.Data data;
    }
    public partial class UiUnitCtrl
    {
        public override void OnCreate()
        {
            view.ipt_default.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.data.paramDic[model.data.name].v = v;
                    parent.Refresh();
                }
            };
            view.ipt_max.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.data.paramDic[model.data.name].max= v;
                    parent.Refresh();
                }
            };
            view.ipt_min.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.data.paramDic[model.data.name].min = v;
                    parent.Refresh();
                }
            };
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            
                var dataCache = parent.model.data;
                view.txt_name.text = model.data.name;
                view.ipt_default.Set(((float)dataCache.paramDic[model.data.name].v).ToString("0.##"));
                view.ipt_min.Set(((float)dataCache.paramDic[model.data.name].min).ToString("0.##"));
                view.ipt_max.Set(((float)dataCache.paramDic[model.data.name].max).ToString("0.##"));
        }


    }

}
