

using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStoryCharacterListArguments.ModStoryCharacterListArgumentsStatic;
using Ui.ModStoryCharacterListArguments.ModStoryCharacterListArgumentsCustom;

namespace Ui.ModStoryCharacterListArguments
{

    public partial class UiModStoryCharacterListArgumentsParam
    {
        public CharacterProductForm.Data data;
        public int selPage;
    }
    public partial class UiModStoryCharacterListArgumentsModel
    {
        public CharacterProductForm.Data data;
        public int selPage;
    }
    public partial class UiModStoryCharacterListArgumentsCtrl
    {

        public override void OnCreate()
        {

            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_static.onClick.AddListener(() =>
            {
                model.selPage = 0;
                Refresh();
            });
            view.btn_custom.onClick.AddListener(() =>
            {
                model.selPage = 1;
                Refresh();
            });

        }
        public override void OnShow()
        {
            model.data = param.data;
            model.selPage = param.selPage;
            Refresh();
        }
        public void Refresh()
        {

            view.page_ModStoryCharacterListArgumentsStatic.SetActive(model.selPage == 0, new UiModStoryCharacterListArgumentsStaticParam()
            {
                data = model.data
            });
            view.page_ModStoryCharacterListArgumentsCustom.SetActive(model.selPage == 1, new UiModStoryCharacterListArgumentsCustomParam()
            {
                data = model.data
            });
            view.img_init.sprite = TextureHelper.transparentSprite;
            view.img_global.sprite = TextureHelper.transparentSprite;
        }
    }

}