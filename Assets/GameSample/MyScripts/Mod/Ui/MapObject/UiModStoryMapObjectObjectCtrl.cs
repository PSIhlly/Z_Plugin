using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryOverview;
using Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject.ModStoryMapObjectObjectAppearance;
using Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject.ModStoryMapObjectObjectConfig;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject
{

    public partial class UiModStoryMapObjectObjectParam
    {
        public MapObjectForm.Data data;
        public int selPage;
    }
    public partial class UiModStoryMapObjectObjectModel
    {
        public MapObjectForm.Data data;

        public int selPage;
    }
    public partial class UiModStoryMapObjectObjectCtrl
    {

        public override void OnCreate()
        {
            view.btn_back.onClick.AddListener(() =>
            {
                parent.SelData(null);
            });
            view.btn_appearance.onClick.AddListener(() =>
            {
                model.selPage = 0;
            });
            view.btn_config.onClick.AddListener(() =>
            {
                model.selPage = 1;
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
            view.page_ModStoryMapObjectObjectAppearance.SetActive(model.selPage == 0, new UiModStoryMapObjectObjectAppearanceParam()
            {
                data = model.data
            });
            view.page_ModStoryMapObjectObjectConfig.SetActive(model.selPage == 1, new UiModStoryMapObjectObjectConfigParam()
            {
                data = model.data
            });
        }
    }

}