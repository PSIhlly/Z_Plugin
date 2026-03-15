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
    public partial class UiModAssetSelectTexWindowParam : UiModAssetSelectWindowParam
    {
        public Action<TexAssetForm.Data> onComplete;
        public Vector2Int sizeLimit;
    }
    public partial class UiModAssetSelectWindowModel
    {
        public UiModAssetSelectWindowParam prm;
        public AssetForm.Data sel;
    }
    public partial class UiModAssetSelectWindowCtrl
    {

        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {

            view.btn_bg.onClick.AddListener(() =>
            {

            });
            view.btn_.onClick.AddListener(() =>
            {
                if (model.prm is UiModAssetSelectTexWindowParam texPrm)
                {
                    texPrm.onComplete?.Invoke((TexAssetForm.Data)model.sel);
                }
                else if (model.prm is UiModAssetSelectAudioWindowParam audioPrm)
                {
                    audioPrm.onComplete?.Invoke((AudioAssetForm.Data)model.sel);
                }
                else if (model.prm is UiModAssetSelectVideoWindowParam videoPrm)
                {
                     videoPrm.onComplete?.Invoke((VideoAssetForm.Data)model.sel);
                }
                Close();
            });
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_import.onClick.AddListener(() =>
            {
                if (model.prm is UiModAssetSelectTexWindowParam texPrm)
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
                Close();
            });
            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);

        }
        public override void OnShow()
        {
            model.sel = null;
            model.prm = param;
            Refresh();
        }
        public void Refresh()
        {
            itemCon.Clear();
            if (model.prm is UiModAssetSelectTexWindowParam texPrm)
            {
                foreach (var data in StoryTexAssetForm.DataById.Values)
                {
                    itemCon.Add(new UiItemParam()
                    {
                        data = data
                    });
                }
            }
            else if (model.prm is UiModAssetSelectAudioWindowParam audioPrm)
            {
                foreach (var data in AudioAssetForm.DataById.Values)
                {
                    itemCon.Add(new UiItemParam()
                    {
                        data = data
                    });
                }
            }
            else if (model.prm is UiModAssetSelectVideoWindowParam videoPrm)
            {
                foreach (var data in VideoAssetForm.DataById.Values)
                {
                    itemCon.Add(new UiItemParam()
                    {
                        data = data
                    });
                }
            }
            
            itemCon.Refresh();

            view.btn_.gameObject.SetActive(model.sel!=null);
        }
    }

    public partial class UiItemParam
    {
        public AssetForm.Data data;
    }
    public partial class UiItemModel
    {
        public UiItemParam prm;
    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.sel = model.prm.data;
                if (parent.model.prm is UiModAssetSelectTexWindowParam texPrm)
                {
                }
                else if (parent.model.prm is UiModAssetSelectAudioWindowParam audioPrm)
                {
                    ((AudioAssetForm.Data)model.prm.data).Play(parent.view.mp_);
                    
                }
                else if (parent.model.prm is UiModAssetSelectVideoWindowParam videoPrm)
                {
                    view.img_.sprite = TextureHelper.transparentSprite;
                }
                parent.Refresh();
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }
        public void Refresh()
        {
            if (parent.model.prm is UiModAssetSelectTexWindowParam texPrm)
            {
                view.img_.sprite = ((TexAssetForm.Data)model.prm.data).GetSprite();
                view.txt_.text = ((TexAssetForm.Data)model.prm.data).name;
            }
            else if (parent.model.prm is UiModAssetSelectAudioWindowParam audioPrm)
            {
                view.img_.sprite = TextureHelper.transparentSprite;
                view.txt_.text = ((AudioAssetForm.Data)model.prm.data).name;
            }
            else if (parent.model.prm is UiModAssetSelectVideoWindowParam videoPrm)
            {
                view.img_.sprite = TextureHelper.transparentSprite;
            }
            view.txt_.text = "";
            view.sta_.ChangeState(parent.model.sel == model.prm.data?1:0);
        }
    }


}