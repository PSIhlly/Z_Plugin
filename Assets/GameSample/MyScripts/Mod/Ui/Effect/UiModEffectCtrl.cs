using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryEffect.ModStoryEffectUnit;

namespace Ui.ModStory.ModStoryEffect
{

    public partial class UiModStoryEffectParam
    {

        public int selPage;
    }
    public partial class UiModStoryEffectModel
    {

        public int selPage;
        public EffectForm.Data data;
    }
    public partial class UiModStoryEffectCtrl
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

        public void SelPage(int id, EffectForm.Data data = null)
        {
            model.selPage = id;
            model.data = data;
            Refresh();
        }
        public void Refresh()
        {

            view.page_ModStoryEffectList.SetShow(model.selPage == 0);
            view.page_ModStoryEffectUnit.SetShow(model.selPage == 1, new UiModStoryEffectUnitParam() { data = model.data });
        }
    }

}