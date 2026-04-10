using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.PlayAsset;
using UnityEngine;
using UnityEngine.Video;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Math;
using Z_Texture;
using Z_Ui.Base;



namespace Ui.PlayAsset
{

    public partial class UiPlayAssetParam
    {

    }
    public partial class UiPlayAssetModel
    {
    }
    public partial class UiPlayAssetCtrl : IZ_Listener<PlayAssetEvent>
    {

        UiContainer<UiImageCtrl> imageCon;
        float time;
        public override void OnCreate()
        {
            imageCon = new UiContainer<UiImageCtrl>(view.go_image);
        }


        public override void OnShow()
        {
            Z_EventHelper.Register(this);
            Refresh();
        }
        public override void OnHide()
        {
            Z_EventHelper.Unregister(this);
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
        public void OnEvent(PlayAssetEvent evt)
        {
            Refresh();
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
            model.prm.data.ctrl = this;
            rect.sizeDelta = new Vector2(model.prm.data.size.x, model.prm.data.size.y);
            view.img_.sprite = TexAssetForm.DataByName.GetDk(model.prm.data.texName, GlobalNameHelper.GetDefaultEventTexName()).GetSprite();
            Refresh();
        }
        public override void OnUpdate()
        {
            model.prm.data.posProgress += Time.deltaTime;
            Refresh();
        }
        public void Refresh()
        {
            var data = model.prm.data;

            if (data.posTime == 0)
            {
                rect.position = data.tarPos;
            }
            else
            {
                rect.position = Vector3.Lerp(data.oldPos, data.tarPos, data.posProgress / data.posTime);
            }
            data.eulerProgress += Time.deltaTime;
            if (data.eulerTime == 0)
            {
                rect.eulerAngles = rect.eulerAngles.NewSetZ(data.tarEuler);
            }
            else
            {
                rect.eulerAngles = rect.eulerAngles.NewSetZ(data.oldEuler + (data.tarEuler - data.oldEuler) * Math.Min(1f, data.eulerProgress / data.eulerTime));
            }

            data.opacityProgress += Time.deltaTime;
            if (data.opacityTime == 0)
            {
                view.img_.color.NewSetA(data.tarOpacity);
            }
            else
            {
                view.img_.color.NewSetA(data.oldOpacity + (data.tarOpacity - data.oldOpacity) * Math.Min(1f, data.opacityProgress / data.opacityTime));
            }
            if (data.removeTime < int.MaxValue)
            {
                data.removeTime -= Time.deltaTime;
                if (data.removeTime <= 0)
                {
                    PlayManager.instance.assetCtrl.Remove(data.uid);
                }
            }

        }
    }


}