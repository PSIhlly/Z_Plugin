using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
namespace Ui.ModStory.ModStoryParameter.ModStoryCharacterParameter
{

    public partial class UiModStoryCharacterParameterParam
    {

    }
    public partial class UiModStoryCharacterParameterModel
    {

    }
    public partial class UiModStoryCharacterParameterCtrl
    {

        UiScrViewContainer<UiArgCtrl> argCon;
        public override void OnCreate()
        {

            argCon = new UiScrViewContainer<UiArgCtrl>(view.go_arg, view.scr_args);

        }
        public override void OnShow()
        {

            Refresh();
        }


        public void Refresh()
        {

            argCon.Clear();
            foreach (var data in CharacterParamForm.DataByName.Values)
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
        public CharacterParamForm.Data data;
    }
    public partial class UiArgModel
    {
        public CharacterParamForm.Data data;
    }
    public partial class UiArgCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateCharacterArg(null);
                parent.Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacterArg(model.data.name);
                parent.Refresh();
            });
            view.ipt_value.onFinishInput+=(s)=>
            {
                model.data.v = StringHelper.ToFloat(s, 0f);
                Refresh();
            };
            view.ipt_name.onFinishInput+=(s)=>
            {
                if (StringHelper.IsUniqueName(CharacterParamForm.DataByName.Keys, s))
                {
                    model.data.name = s;
                }
                Refresh();
            };

        }
        public override void OnShow()
        {
            model.data=param.data;
            Refresh();
        }

        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data == null ? 0 : 1);

            if (model.data != null)
            {
                view.ipt_name.Set(model.data.name);
                view.ipt_value.Set(model.data.v.ToString("0.##"));
            }

        }
    }


}