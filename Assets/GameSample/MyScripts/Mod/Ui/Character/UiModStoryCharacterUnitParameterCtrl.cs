/*using Form;
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
            foreach (var data in model.data.paramDic.Values)
            {
                itemCon.Add(new UiItemParam()
                {
                    data = data
                });
            }
            itemCon.Refresh();
        }
    }

    public partial class UiItemParam
    {
        public CharacterParamForm.Data data;
    }
    public partial class UiItemModel
    {
        public CharacterParamForm.Data data;
    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {

            view.ipt_min.onFinishInput+=(s) =>
            {
                model.data.min = StringHelper.ToFloat(s, 0);
            };
            view.ipt_value.onFinishInput+=(s)=>
            {
                model.data.v = StringHelper.ToFloat(s, 0);

            };
            view.ipt_max.onFinishInput+=(s)=>
            {
                model.data.max = StringHelper.ToFloat(s, 0);

            };

        }
        public override void OnShow()
        {

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


}*/