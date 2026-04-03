using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitAppearance.ModStoryCharacterUnitAppearanceList;
using Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitAppearance.ModStoryCharacterUnitAppearanceUnit;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitAppearance
{

    public partial class UiModStoryCharacterUnitAppearanceParam
    {

        public CharacterProductForm.Data data;
        public CharacterAnimForm.Data animData;
        public int selPage;
    }
    public partial class UiModStoryCharacterUnitAppearanceModel
    {

        public CharacterProductForm.Data data;
        public CharacterAnimForm.Data animData;
        public int selPage;
    }
    public partial class UiModStoryCharacterUnitAppearanceCtrl
    {

        public override void OnCreate()
        {


        }
        public override void OnShow()
        {
            model.selPage = 0;
            if (param != null)
            {
                model.selPage = param.selPage;
                model.data = param.data;
            }
            Refresh();
        }
        public void SelPage(int id,CharacterAnimForm.Data animData=null)
        {
            model.selPage = id;
            model.animData = animData;
            Refresh();
        }
        public void Refresh()
        {

            view.page_ModStoryCharacterUnitAppearanceList.SetShow(model.selPage == 0,new UiModStoryCharacterUnitAppearanceListParam()
            {
                data = model.data  
            });
            view.page_ModStoryCharacterUnitAppearanceUnit.SetShow(model.selPage == 1, new UiModStoryCharacterUnitAppearanceUnitParam()
            {
                data = model.animData,
                ch = model.data
            });
        }
    }

}