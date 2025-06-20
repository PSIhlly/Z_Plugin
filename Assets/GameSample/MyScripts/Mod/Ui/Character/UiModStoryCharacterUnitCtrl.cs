using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryOverview;
using Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitOverview;
using Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitParameter;
using Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitAppearance;
using Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitConfig;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit
{

    public partial class UiModStoryCharacterUnitParam
    {
        public CharacterProductForm.Data data;
        public int selPage;
    }
    public partial class UiModStoryCharacterUnitModel
    {
        public CharacterProductForm.Data data;

        public int selPage;
    }
    public partial class UiModStoryCharacterUnitCtrl
    {

        public override void OnCreate()
        {

            view.btn_back.onClick.AddListener(() =>
            {
                parent.SelPage(0);
            });
            view.btn_overview.onClick.AddListener(() =>
            {
                model.selPage = 0;
            });
            view.btn_parameter.onClick.AddListener(() =>
            {
                model.selPage = 1;
            });
            view.btn_appearance.onClick.AddListener(() =>
            {
                model.selPage = 2;
            });
            view.btn_config.onClick.AddListener(() =>
            {
                model.selPage = 3;
            });

        }
        public override void OnShow()
        {

            if (param != null)
            { 
                model.selPage = param.selPage; 
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {

            view.page_ModStoryCharacterUnitOverview.SetActive(model.selPage == 0,new UiModStoryCharacterUnitOverviewParam()
            {
                data=model.data
            });
            view.page_ModStoryCharacterUnitParameter.SetActive(model.selPage == 1, new UiModStoryCharacterUnitParameterParam()
            {
                data = model.data
            });
            view.page_ModStoryCharacterUnitAppearance.SetActive(model.selPage == 2, new UiModStoryCharacterUnitAppearanceParam()
            {
                data = model.data
            });
            view.page_ModStoryCharacterUnitConfig.SetActive(model.selPage == 3, new UiModStoryCharacterUnitConfigParam() 
            {
                data = model.data 
            });
        }
    }

}