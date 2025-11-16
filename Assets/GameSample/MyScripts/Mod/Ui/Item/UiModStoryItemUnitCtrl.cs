using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryOverview;
using Ui.ModStory.ModStoryItem.ModStoryItemUnit.ModStoryItemUnitParameter;
using Ui.ModStory.ModStoryItem.ModStoryItemUnit.ModStoryItemUnitOverview;
using Ui.ModStory.ModStoryItem.ModStoryItemUnit.ModStoryItemUnitAppearance;
using Ui.ModStory.ModStoryItem.ModStoryItemUnit.ModStoryItemUnitConfig;

namespace Ui.ModStory.ModStoryItem.ModStoryItemUnit
{

    public partial class UiModStoryItemUnitParam
    {
        public ItemProductForm.Data data;
        public int selPage;
    }
    public partial class UiModStoryItemUnitModel
    {
        public ItemProductForm.Data data;

        public int selPage;
    }
    public partial class UiModStoryItemUnitCtrl
    {

        public override void OnCreate()
        {

            view.btn_back.onClick.AddListener(() =>
            {
                parent.SelPage(0);
            });
            view.btn_overview.onClick.AddListener(() =>
            {
                model.selPage = 0;
                Refresh();
            });
            view.btn_parameter.onClick.AddListener(() =>
            {
                model.selPage = 1;
                Refresh();

            });
            view.btn_appearance.onClick.AddListener(() =>
            {
                model.selPage = 2;
                Refresh();

            });
            view.btn_config.onClick.AddListener(() =>
            {
                model.selPage = 3;
                Refresh();

            });

        }
        public override void OnShow()
        {

            if (param != null)
            {
                model.selPage = param.selPage;
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {

            view.page_ModStoryItemUnitOverview.SetShow(model.selPage == 0, new UiModStoryItemUnitOverviewParam()
            {
                data = model.data
            });
            view.sta_overview.ChangeState(model.selPage == 0 ? 1 : 0);
            view.page_ModStoryItemUnitParameter.SetShow(model.selPage == 1, new UiModStoryItemUnitParameterParam()
            {
                data = model.data
            });
            view.sta_parameter.ChangeState(model.selPage == 1 ? 1 : 0);

            view.page_ModStoryItemUnitAppearance.SetShow(model.selPage == 2, new UiModStoryItemUnitAppearanceParam()
            {
                data = model.data
            });
            view.sta_appearance.ChangeState(model.selPage == 2 ? 1 : 0);

            view.page_ModStoryItemUnitConfig.SetShow(model.selPage == 3, new UiModStoryItemUnitConfigParam()
            {
                data = model.data
            });
            view.sta_config.ChangeState(model.selPage == 3 ? 1 : 0);

        }
    }

}