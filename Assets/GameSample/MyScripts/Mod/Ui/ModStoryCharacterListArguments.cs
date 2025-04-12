

using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;

namespace Ui.ModStoryCharacterListArguments
{

    public partial class UiModStoryCharacterListArgumentsParam
    {

        public int selPage;
    }
    public partial class UiModStoryCharacterListArgumentsModel
    {

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
                model.selPage = 0;
                Refresh();
            });

        }
        public override void OnShow()
        {

            if (param != null)
                model.selPage = param.selPage;
            Refresh();
        }
        public void Refresh()
        {

            view.page_ModStoryCharacterListArgumentsStatic.SetActive(model.selPage == 0);
            view.page_ModStoryCharacterListArgumentsCustom.SetActive(model.selPage == 1);
            view.img_init.sprite = TextureHelper.transparentSprite;
            view.img_global.sprite = TextureHelper.transparentSprite;
        }
    }

}