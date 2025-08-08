

using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;

namespace Ui.ModStory.ModStoryMap
{

    public partial class UiModStoryMapParam
    {

        public int selPage;
    }
    public partial class UiModStoryMapModel
    {

        public int selPage;
    }
    public partial class UiModStoryMapCtrl
    {

        public override void OnCreate()
        {

            view.btn_map.onClick.AddListener(() =>
            {
                model.selPage = 0;
            });
            view.btn_scene.onClick.AddListener(() =>
            {
                model.selPage = 1;
            });
            view.btn_config.onClick.AddListener(() =>
            {
                model.selPage = 2;
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