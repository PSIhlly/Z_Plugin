

using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;

namespace Ui.ModStory.ModStoryParameter
{

    public partial class UiModStoryParameterParam
    {

        public int selPage;
    }
    public partial class UiModStoryParameterModel
    {

        public int selPage;
    }
    public partial class UiModStoryParameterCtrl
    {

        public override void OnCreate()
        {

            view.btn_globalParameter.onClick.AddListener(() =>
            {
                model.selPage = 0;
            });
            view.btn_characterParameter.onClick.AddListener(() =>
            {
                model.selPage = 1;
            });
            view.btn_itemParameter.onClick.AddListener(() =>
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
            model.selPage = 0;
            if (param != null)
                model.selPage = param.selPage;
            Refresh();
        }
        public void Refresh()
        {
            view.page_ModStoryGlobalParameter.SetActive(model.selPage == 0);
            view.page_ModStoryCharacterParameter.SetActive(model.selPage == 1);
            view.page_ModStoryItemParameter.SetActive(model.selPage == 2);
            view.page_ModStoryConfig.SetActive(model.selPage == 3);
        }
    }

}