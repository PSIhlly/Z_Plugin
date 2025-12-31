using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Texture;
using Z_Ui.Base;


namespace Ui.ModAssetSelectWindow
{
    public partial class UiModAssetSelectAudioWindowParam : UiModAssetSelectWindowParam
    {
        public Action<AudioAssetForm.Data> onComplete;
    }
    public partial class UiModAssetSelectVideoWindowParam : UiModAssetSelectWindowParam
    {
        public Action<VideoAssetForm.Data> onComplete;
    }
    public partial class UiModAssetSelectTexWindowParam: UiModAssetSelectWindowParam
    {
        public Action<TexAssetForm.Data> onComplete;
        public Vector2Int sizeLimit;
    }
    public partial class UiModAssetSelectWindowModel
    {
        public UiModAssetSelectWindowParam prm;
    }
    public partial class UiModAssetSelectWindowCtrl
    {

        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {

            view.btn_bbg.onClick.AddListener(() =>
            {

            });
            view.btn_.onClick.AddListener(() =>
            {

            });
            view.btn_close.onClick.AddListener(() =>
            {

            });
            view.btn_extern.onClick.AddListener(() =>
            {

            });
            view.btn_internal.onClick.AddListener(() =>
            {

            });
            view.btn_import.onClick.AddListener(() =>
            {
                if(model.prm is UiModAssetSelectTexWindowParam texPrm)
                {
                    AssetManager.instance.texCtrl.Select(texPrm.sizeLimit, (data) =>
                    {
                        texPrm.onComplete?.Invoke(data);
                    });
                }
                else if (model.prm is UiModAssetSelectAudioWindowParam audioPrm)
                {
                    AssetManager.instance.audioCtrl.Select((data) =>
                    {
                        audioPrm.onComplete?.Invoke(data);
                    });
                }
                else if (model.prm is UiModAssetSelectVideoWindowParam videoPrm)
                {
                    AssetManager.instance.videoCtrl.Select((data) =>
                    {
                        videoPrm.onComplete?.Invoke(data);
                    });
                }
            });
            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);

        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }
        public void Refresh()
        {

            view.txt_externPath.text = "";
            itemCon.Clear();
            for (int i = 0, icnt = ; i < icnt; i++)
            {
                itemCon.Add(new UiItemParam()
                {

                });
            }
            itemCon.Refresh();
        }
    }

    public partial class UiitemParam
    {

    }
    public partial class UiitemModel
    {

    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {

            });

        }
        public override void OnShow()
        {

            Refresh();
        }
        public void Refresh()
        {

            view.img_.sprite = TextureHelper.transparentSprite;
            view.txt_.text = "";
        }
    }


}