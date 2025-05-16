

using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStoryItemListArguments.ModStoryItemListArgumentsStatic;
using Ui.ModStoryItemListArguments.ModStoryItemListArgumentsCustom;

namespace Ui.ModStoryItemListArguments
{

    public partial class UiModStoryItemListArgumentsParam
    {
        public ItemProductForm.Data data;
        public int selPage;
    }
    public partial class UiModStoryItemListArgumentsModel
    {
        public ItemProductForm.Data data;
        public int selPage;
    }
    public partial class UiModStoryItemListArgumentsCtrl
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

            view.page_ModStoryItemListArgumentsStatic.SetActive(model.selPage == 0, new UiModStoryItemListArgumentsStaticParam()
            {
                data = model.data
            });
            view.page_ModStoryItemListArgumentsCustom.SetActive(model.selPage == 1, new UiModStoryItemListArgumentsCustomParam()
            {
                data = model.data
            });
            view.img_init.sprite = TextureHelper.transparentSprite;
            view.img_global.sprite = TextureHelper.transparentSprite;
        }
    }

}