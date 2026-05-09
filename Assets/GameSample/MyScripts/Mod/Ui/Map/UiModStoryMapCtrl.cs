

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
            if(!GameManager.instance.curProgress.enableLargeMap)
            {
                model.selPage = 1;
            }
            if (param != null)
                model.selPage = param.selPage;
            Refresh();
        }

        public void Refresh()
        {
            view.btn_map.gameObject.SetActive(GameManager.instance.curProgress.enableLargeMap);
            view.page_ModStoryMapMap.SetShow(model.selPage == 0 );
            view.sta_map.ChangeState(model.selPage == 0 ? 1 : 0);

            view.page_ModStoryMapScene.SetShow(model.selPage == 1);
            view.sta_scene.ChangeState(model.selPage == 1 ? 1 : 0);

            view.page_ModStoryMapConfig.SetShow(model.selPage == 2);
            view.sta_config.ChangeState(model.selPage == 2 ? 1 : 0);

        }
    }

}