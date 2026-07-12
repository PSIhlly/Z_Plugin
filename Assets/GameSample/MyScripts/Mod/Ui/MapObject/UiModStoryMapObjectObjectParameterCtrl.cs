using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
using Z_Code;
using Z_Text;
using Z_DesignStyle;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject.ModStoryMapObjectObjectParameter
{

    public partial class UiModStoryMapObjectObjectParameterParam
    {
        public MapObjectForm.Data data;
    }
    public partial class UiModStoryMapObjectObjectParameterModel
    {
        public MapObjectForm.Data data;
    }
    public partial class UiModStoryMapObjectObjectParameterCtrl
    {
        UiScrViewContainer<UiArgIptCtrl> argIptCon;
        public override void OnCreate()
        {
            argIptCon = new UiScrViewContainer<UiArgIptCtrl>(this, view.go_argIpt, view.scr_argIpts);
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {

            argIptCon.Clear();
            foreach (var data in MapObjectParamForm.DataByName.Values.OrderBy(d => d.uid))
            {
                if (!model.data.paramDic.ContainsKey(data.name))
                {
                    model.data.paramDic[data.name] = data.Copy();
                }
                argIptCon.Add(new UiArgIptParam()
                {
                    data = model.data.paramDic[data.name]
                });
            }
            argIptCon.Refresh();
        }
    }

    public partial class UiArgIptParam
    {
        public MapObjectParamForm.Data data;
    }
    public partial class UiArgIptModel
    {
        public MapObjectParamForm.Data data;
    }
    public partial class UiArgIptCtrl
    {

        public override void OnCreate()
        {

            view.btn_min.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseSceneObjectParam(TextManager.instance.GetTxt("choose value min parameter"), (item) =>
                 {
                     model.data.min = item.content;
                     Refresh();
                 }, new Z_Ui.Notify.EntryItem()
                 {
                     content = ""
                 });
            });
            view.btn_value.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseBasicCmd(model.data.GetValue(), () =>
                {
                    Refresh();
                });
            });
            view.btn_max.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseSceneObjectParam(TextManager.instance.GetTxt("choose value max parameter"), (item) =>
                {
                    model.data.max = item.content;
                    Refresh();
                }, new Z_Ui.Notify.EntryItem()
                {
                    content = ""
                });
            });

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {

            view.txt_.text = model.data.name;
            view.txt_min.text = model.data.min;
            view.txt_value.text = (model.data.GetValue().GetBoxContent());
            view.txt_max.text = model.data.max;
        }
    }


}
