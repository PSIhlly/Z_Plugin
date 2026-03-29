using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Ui.EventChoose;
using Ui.ModStory.ModStorySkill.ModStorySkillUnit.ModStorySkillUnitOverview;
using Ui.ModStory.ModStorySkill.ModStorySkillUnit.ModStorySkillUnitParameter;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Math;
using Z_String;
using Z_Text;
using Z_Texture;
using Z_Ui.Base;

namespace Ui.ModStory.ModStorySkill.ModStorySkillUnit
{

    public partial class UiModStorySkillUnitParam
    {
        public SkillProductForm.Data data;
        public int selPage;
    }
    public partial class UiModStorySkillUnitModel
    {
        public SkillProductForm.Data data;
        public int selPage;
    }
    public partial class UiModStorySkillUnitCtrl
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

            view.page_ModStorySkillUnitOverview.SetShow(model.selPage == 0, new UiModStorySkillUnitOverviewParam()
            {
                data = model.data
            });
            view.sta_overview.ChangeState(model.selPage == 0 ? 1 : 0);
            view.page_ModStorySkillUnitParameter.SetShow(model.selPage == 1, new UiModStorySkillUnitParameterParam()
            {
                data = model.data
            });
            view.sta_parameter.ChangeState(model.selPage == 1 ? 1 : 0);

        }
    }

}
