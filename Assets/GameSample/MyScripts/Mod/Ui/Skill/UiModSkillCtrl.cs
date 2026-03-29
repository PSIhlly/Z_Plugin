using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStorySkill.ModStorySkillUnit;

namespace Ui.ModStory.ModStorySkill
{

    public partial class UiModStorySkillParam
    {

        public int selPage;
    }
    public partial class UiModStorySkillModel
    {

        public int selPage;
        public SkillProductForm.Data data;
    }
    public partial class UiModStorySkillCtrl
    {

        public override void OnCreate()
        {


        }
        public override void OnShow()
        {
            model.selPage = 0;
            if (param != null)
                model.selPage = param.selPage;
            Refresh();
        }

        public void SelPage(int id, SkillProductForm.Data data = null)
        {
            model.selPage = id;
            model.data = data;
            Refresh();
        }
        public void Refresh()
        {

            view.page_ModStorySkillList.SetShow(model.selPage == 0);
            view.page_ModStorySkillUnit.SetShow(model.selPage == 1, new UiModStorySkillUnitParam() { data = model.data });
        }
    }

}