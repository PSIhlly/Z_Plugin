using Form;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.PlayAsset;
using UnityEngine;
using UnityEngine.Video;
using Z_DesignStyle;
using Z_Math;
using Z_Texture;
using Z_Ui.Base;
using Z_Video;


namespace Ui.PlayAsset
{

    public partial class UiPlayAssetParam
    {

    }
    public partial class UiPlayAssetModel
    {
    }
    public partial class UiPlayAssetCtrl
    {

        UiContainer<UiImageCtrl> imageCon;
        public override void OnCreate()
        {

            imageCon = new UiContainer<UiImageCtrl>(view.go_image);

        }
        public override void OnShow()
        {
        }
        public override void OnUpdate()
        {
            Refresh();
        }
        public void Refresh()
        {

            imageCon.Clear();
            foreach (var data in ImageUiItemForm.DataByUid.Values)
            {
                imageCon.Add(new UiImageParam()
                {
                    data = data
                });
            }
            imageCon.Refresh();
        }
    }

    public partial class UiImageParam
    {
        public ImageUiItemForm.Data data;
    }
    public partial class UiImageModel
    {
        public UiImageParam prm;
    }
    public partial class UiImageCtrl
    {

        public override void OnCreate()
        {


        }
        public override void OnShow()
        {
            model.prm = param;
            rect.rect.Set(0, 0, model.prm.data.size.x, model.prm.data.size.y);
            view.img_image.sprite = StoryTexAssetForm.DataByName.GetDk(model.prm.data.texName, GlobalNameHelper.GetDefaultTexName()).GetSprite();
            Refresh();
        }
        public void Refresh()
        {
            var data = model.prm.data;
            rect.position = Graph.GetRealPos(data.oldPos + (data.tarPos - data.oldPos) * (data.posProgress / data.posTime), parent.rect);
            rect.eulerAngles = rect.eulerAngles.NewSetZ(data.oldEuler + (data.tarEuler - data.oldEuler) * (data.eulerProgress / data.eulerTime));
            view.img_image.color.NewSetA(data.oldOpacity + (data.tarOpacity - data.oldOpacity) * (data.opacityProgress / data.opacityTime));
            data.removeTime -= Time.deltaTime;
            if (data.removeTime <= 0)
            {
                PlayManager.instance.assetCtrl.Remove(data.uid);
            }
        }
    }


}