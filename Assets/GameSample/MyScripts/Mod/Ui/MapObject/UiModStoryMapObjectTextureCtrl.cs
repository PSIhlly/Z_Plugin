using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Ui.ModStory.ModStoryOverview;
using Ui.ModStory.ModStoryMapObject.ModStoryMapObjectTexture.ModStoryMapObjectTextureAppearance;
using Ui.ModStory.ModStoryMapObject.ModStoryMapObjectTexture.ModStoryMapObjectTextureConfig;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectTexture
{

    public partial class UiModStoryMapObjectTextureParam
    {
        public MapTextureForm.Data data;
        public int selPage;
    }
    public partial class UiModStoryMapObjectTextureModel
    {
        public MapTextureForm.Data data;

        public int selPage;
    }
    public partial class UiModStoryMapObjectTextureCtrl
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
                Refresh();
            });
            view.btn_config.onClick.AddListener(() =>
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
        public override void OnHide()
        {
            GameManager.instance.saveCtrl.SaveMaterial(ModManager.instance.GetStoryCoreFolder());
        }
        public void Refresh()
        {

            view.page_ModStoryMapObjectTextureAppearance.SetActive(model.selPage == 0, new UiModStoryMapObjectTextureAppearanceParam()
            {
                data = model.data
            });
            view.page_ModStoryMapObjectTextureConfig.SetActive(model.selPage == 1, new UiModStoryMapObjectTextureConfigParam()
            {
                data = model.data
            });
        }
    }

}