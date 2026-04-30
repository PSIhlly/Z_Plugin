
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
            view.btn_characterParameter.onClick.AddListener(() =>
            {
                model.selPage = 0;
                Refresh();
            });
            view.btn_itemParameter.onClick.AddListener(() =>
            {
                model.selPage = 1;
                Refresh();
            });
            view.btn_skillParameter.onClick.AddListener(() =>
            {
                model.selPage = 2;
                Refresh();
            });
            view.btn_sceneObjectParameter.onClick.AddListener(() =>
            {
                model.selPage = 3;
                Refresh();
            });
            view.btn_config.onClick.AddListener(() =>
            {
                model.selPage = 4;
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

        public void Refresh()
        {
            view.page_ModStoryCharacterParameter.SetShow(model.selPage == 0);
            view.sta_characterParameter.ChangeState(model.selPage == 0 ? 1 : 0);

            view.page_ModStoryItemParameter.SetShow(model.selPage == 1);
            view.sta_itemParameter.ChangeState(model.selPage == 1 ? 1 : 0);

            view.page_ModStorySkillParameter.SetShow(model.selPage == 2);
            view.sta_skillParameter.ChangeState(model.selPage == 2 ? 1 : 0);

            view.page_ModStorySceneObjectParameter.SetShow(model.selPage == 3);
            view.sta_sceneObjectParameter.ChangeState(model.selPage == 3 ? 1 : 0);

            view.page_ModStoryConfig.SetShow(model.selPage == 4);
            view.sta_config.ChangeState(model.selPage == 4 ? 1 : 0);


            view.btn_skillParameter.gameObject.SetActive(GameManager.instance.curProgress.enableSkill);

        }
    }

}
