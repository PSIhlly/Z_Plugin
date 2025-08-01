using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem;

namespace Ui.ModStory.ModStoryOverview
{

    public partial class UiModStoryOverviewParam
    {

    }
    public partial class UiModStoryOverviewModel
    {

    }
    public partial class UiModStoryOverviewCtrl:IZ_Listener<AssetEvent>
    {

        public override void OnCreate()
        {

            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportStoryTex();
            });
            view.ipt_name.onFinishInput += (s)=>
            {
                StoryForm.DataById[1].name = s;
            };
            view.ipt_introduction.onFinishInput+=(s)=>
            {
                StoryForm.DataById[1].desc = s;
            };

        }

        public void OnEvent(AssetEvent evt)
        {
            Refresh();
        }

        public override void OnShow()
        {

            Refresh();
        }
        public override void Close()
        {
            GameManager.instance.saveCtrl.SaveOverview(ModManager.instance.GetStoryCoreFolder());
            base.Close();

        }
        public void Refresh()
        {
            view.img_image.sprite = StoryTexAssetForm.DataByName[StoryForm.DataById[1].icon].sprite;
            view.ipt_introduction.Set(StoryForm.DataById[1].desc);
            view.ipt_name.Set(StoryForm.DataById[1].name);
        }
    }

}