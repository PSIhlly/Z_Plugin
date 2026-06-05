using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DataSystem.Form;
using Z_Text;
using Z_Math;
using Z_String;
using Z_DesignStyle;

namespace Ui.ModStory.ModStoryMap.ModStoryMapConfig
{

    public partial class UiModStoryMapConfigParam
    {

    }
    public partial class UiModStoryMapConfigModel
    {

    }
    public partial class UiModStoryMapConfigCtrl
    {

        public override void OnCreate()
        {
            view.btn_initScene.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseScene(TextManager.instance.GetTxt("ChooseScene"), (data) =>
                {
                    GameManager.instance.curProgress.targetScene = (data.uid, GameManager.instance.curProgress.targetScene.Item2);
                    Refresh();
                });
            });

            view.ipt_initPosSetX.onFinishInput += (s) =>
            {
                var pos = GameManager.instance.curProgress.targetScene.Item2;
                GameManager.instance.curProgress.targetScene = (GameManager.instance.curProgress.targetScene.Item1, pos.NewSetX(StringHelper.ToFloat(s, 0)));
                Refresh();
            };
            view.ipt_initPosSetZ.onFinishInput += (s) =>
            {
                var pos = GameManager.instance.curProgress.targetScene.Item2;
                GameManager.instance.curProgress.targetScene = (GameManager.instance.curProgress.targetScene.Item1, pos.NewSetZ(StringHelper.ToFloat(s, 0)));
                Refresh();
            };
            view.ipt_initPosSetY.onFinishInput += (s) =>
            {
                var pos = GameManager.instance.curProgress.targetScene.Item2;
                GameManager.instance.curProgress.targetScene = (GameManager.instance.curProgress.targetScene.Item1, pos.NewSetY(StringHelper.ToFloat(s, 0)));
                Refresh();
            };

        }
        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            var scene = SceneForm.DataByUid.GetDv(GameManager.instance.curProgress.targetScene.Item1, null);
            view.txt_initScene.text = scene == null ? "" : scene.name;
            var pos = GameManager.instance.curProgress.targetScene.Item2;
            view.ipt_initPosSetX.Set(pos.x.ToString());
            view.ipt_initPosSetZ.Set(pos.z.ToString());
            view.ipt_initPosSetY.Set(pos.y.ToString());
        }
    }

}
