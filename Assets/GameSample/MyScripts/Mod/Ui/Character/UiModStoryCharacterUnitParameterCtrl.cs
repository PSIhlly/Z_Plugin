using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitParameter
{

    public partial class UiModStoryCharacterUnitParameterParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitParameterModel
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitParameterCtrl
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
        public CharacterParamForm.Data data;
    }
    public partial class UiArgIptModel
    {
        public CharacterParamForm.Data data;
    }
    public partial class UiArgIptCtrl
    {

        public override void OnCreate()
        {

            view.ipt_min.onFinishInput += (s) =>
            {
                model.data.min = StringHelper.ToFloat(s, 0);
                Refresh();
            };
            view.ipt_value.onFinishInput += (s) =>
            {
                model.data.v = StringHelper.ToFloat(s, 0);
                Refresh();
            };
            view.ipt_max.onFinishInput += (s) =>
            {
                model.data.max = StringHelper.ToFloat(s, 0);
                Refresh();
            };

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {

            view.txt_.text = model.data.name;
            view.ipt_min.Set(model.data.min.ToString());
            view.ipt_value.Set(model.data.v.ToString());
            view.ipt_max.Set(model.data.max.ToString());
        }
    }


}