using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
using Z_Text;
namespace Ui.ModStory.ModStoryParameter.ModStorySkillParameter
{

    public partial class UiModStorySkillParameterParam
    {

    }
    public partial class UiModStorySkillParameterModel
    {

    }
    public partial class UiModStorySkillParameterCtrl
    {

        UiScrViewContainer<UiArgCtrl> argCon;
        public override void OnCreate()
        {

            argCon = new UiScrViewContainer<UiArgCtrl>(this, view.go_arg, view.scr_args);

        }
        public override void OnShow()
        {
            Refresh();
        }
       
        public void Refresh()
        {

            argCon.Clear();
            foreach (var data in SkillParamForm.DataByName.Values.OrderBy(d => d.uid))
            {
                argCon.Add(new UiArgParam()
                {
                    data = data
                });
            }
            argCon.Add(new UiArgParam()
            {
                data = null
            });
            argCon.Refresh();
        }
    }


    public partial class UiArgParam
    {
        public SkillParamForm.Data data;
    }
    public partial class UiArgModel
    {
        public SkillParamForm.Data data;
    }
    public partial class UiArgCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateSkillArg(null);
                parent.Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteSkillArg(model.data.name);
                parent.Refresh();
            });
            view.ipt_value.onFinishInput += (s) =>
            {
                model.data.SetValue(StringHelper.ToFloat(s, 0f));
                Refresh();
            };
            view.ipt_name.onFinishInput += (s) =>
            {
                if (StringHelper.IsUniqueName(SkillParamForm.DataByName.Keys, s))
                {
                    ModManager.instance.assetCtrl.RenameSkillParam(model.data.name, s);
                }
                Refresh();
            };
            view.dp_.onFinishSelect = (v) =>
            {
                model.data.showType = (ParamShowType)v;
                Refresh();
            };
            view.dp_.ClearOptions();
            foreach (ParamShowType type in Enum.GetValues(typeof(ParamShowType)))
            {
                view.dp_.options.Add(new TMPro.TMP_Dropdown.OptionData(TextManager.instance.GetTxt(type.ToString())));
            }
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
                view.ipt_name.Set(model.data.name);
                view.ipt_value.Set(model.data.GetValue().num.ToString("0.##"));
                view.dp_.Set((int)model.data.showType);
            }

        }
    }


}
