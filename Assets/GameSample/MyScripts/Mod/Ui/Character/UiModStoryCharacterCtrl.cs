using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit;
using UnityEngine;

namespace Ui.ModStory.ModStoryCharacter
{

    public partial class UiModStoryCharacterParam
    {

        public int selPage;
    }
    public partial class UiModStoryCharacterModel
    {

        public int selPage;
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterCtrl
    {

        public override void OnCreate()
        {


        }
        public override void OnShow()
        {
            model.selPage = 0;
            if (param != null)
                model.selPage = param.selPage;
            Refresh();
        }
        public override void OnHide()
        {
            GameManager.instance.saveCtrl.SaveCharacter(ModManager.instance.GetStoryCoreFolder());
        }
        public void SelPage(int id, CharacterProductForm.Data data=null)
        {
            model.selPage = id;
            model.data = data;
            Refresh();
        }
        public void Refresh()
        {

            view.page_ModStoryCharacterList.SetActive(model.selPage == 0);
            view.page_ModStoryCharacterUnit.SetActive(model.selPage == 1, new UiModStoryCharacterUnitParam() { data = model.data });
        }
    }

}