using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryEvent;

namespace Ui.ModStory.ModStoryEvent
{

    public partial class UiModStoryEventParam
    {

        public int selPage;
    }
    public partial class UiModStoryEventModel
    {

        public int selPage;
    }
    public partial class UiModStoryEventCtrl
    {

        public override void OnCreate()
        {
            view.btn_customEvent.onClick.AddListener(() =>
            {
                model.selPage = 0;
                Refresh();

            });
            view.btn_globalEvent.onClick.AddListener(() =>
            {
                model.selPage = 1;
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
            GameManager.instance.saveCtrl.SaveEvent(ModManager.instance.GetStoryCoreFolder());
        }
        public void SelPage(int id)
        {
            model.selPage = id;
            Refresh();
        }
        public void Refresh()
        {
            view.page_ModStoryEventCustom.SetActive(model.selPage == 0);
            view.sta_customEvent.ChangeState(model.selPage == 0 ? 1 : 0);
            view.page_ModStoryEventConfig.SetActive(model.selPage == 1);
            view.sta_globalEvent.ChangeState(model.selPage == 1 ? 1 : 0);

        }
    }

}