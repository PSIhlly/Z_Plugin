using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem;
using Z_DataSystem.Form;

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
                GameManager.instance.curStory.name = s;
            };
            view.ipt_introduction.onFinishInput+=(s)=>
            {
                GameManager.instance.curStory.desc = s;
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
            GameManager.instance.saveCtrl.SaveOverview(GameManager.instance.curStory.id);
            base.Close();

        }
        public void Refresh()
        {
            view.img_image.sprite = TexAssetForm.DataByName[GameManager.instance.curStory.icon].sprite;
            view.ipt_introduction.Set(GameManager.instance.curStory.desc);
            view.ipt_name.Set(GameManager.instance.curStory.name);
        }
    }

}