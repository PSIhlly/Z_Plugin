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
            argIptCon = new UiScrViewContainer<UiArgIptCtrl>(view.go_argIpt, view.scr_argIpts);
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {

            argIptCon.Clear();
            foreach (var data in model.data.paramDic.Values)
            {
                argIptCon.Add(new UiArgIptParam()
                {
                    data = data
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
                ModManager.instance.assetCtrl.ChooseBasicCmd(model.data.GetMin(), () =>
                {
                    Refresh();
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
                ModManager.instance.assetCtrl.ChooseBasicCmd(model.data.GetMax(), () =>
                {
                    Refresh();
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
            view.txt_min.text = (model.data.GetMin().GetBoxContent());
            view.txt_value.text = (model.data.GetValue().GetBoxContent());
            view.txt_max.text = (model.data.GetMax().GetBoxContent());
        }
    }


}
