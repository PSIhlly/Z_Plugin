

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
                Refresh();
            });
            view.btn_scene.onClick.AddListener(() =>
            {
                model.selPage = 1;
                Refresh();
            });
            view.btn_config.onClick.AddListener(() =>
            {
                model.selPage = 2;
                Refresh();
            });

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
            GameManager.instance.saveCtrl.SaveScene(ModManager.instance.GetStoryCoreFolder());
            GameManager.instance.saveCtrl.SaveConfig(ModManager.instance.GetStoryCoreFolder());
        }
        public void Refresh()
        {
            view.page_ModStoryMapMap.SetActive(model.selPage == 0);
            view.sta_map.ChangeState(model.selPage == 0 ? 1 : 0);

            view.page_ModStoryMapScene.SetActive(model.selPage == 1);
            view.sta_scene.ChangeState(model.selPage == 1 ? 1 : 0);

            view.page_ModStoryMapConfig.SetActive(model.selPage == 2);
            view.sta_config.ChangeState(model.selPage == 2 ? 1 : 0);

        }
    }

}