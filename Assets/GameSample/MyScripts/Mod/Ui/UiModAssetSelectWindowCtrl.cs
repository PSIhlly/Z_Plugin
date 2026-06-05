using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Text;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui.Notify;


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
        public string curLab;
        public bool isSetLabelMode;
    }
    public partial class UiModAssetSelectWindowCtrl : IZ_Listener<AssetEvent>
    {

        UiScrViewContainer<UiItemCtrl> itemCon;
        UiScrViewContainer<UiLabCtrl> labCon;
        public override void OnCreate()
        {
            this.Register();
            view.btn_delete.onClick.AddListener(() =>
            {
                if (model.sel != null)
                {
                    AssetForm.RemoveData(model.sel.id);
                    model.sel = null;
                    Refresh();
                }
            });

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
                Z_EventHelper.Invoke(new AssetEvent()
                {
                    importAssetName = model.sel.name
                });
                Close();
            });
            view.btn_close.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_setLabel.onClick.AddListener(() =>
            {
                if (model.sel != null)
                {
                    model.isSetLabelMode = !model.isSetLabelMode;
                    Refresh();
                }
            });

            itemCon = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            labCon = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_labs);

        }
        public override void OnShow()
        {
            model.prm = param;
            model.isSetLabelMode = false;
            if(model.curLab!=null)
            {
                if (model.prm is UiModAssetSelectTexWindowParam && !StoryTexAssetForm.DatasByLab.ContainsKey(model.curLab))
                {
                    model.curLab = null;
                }
                if (model.prm is UiModAssetSelectAudioWindowParam && !StoryAudioAssetForm.DatasByLab.ContainsKey(model.curLab))
                {
                    model.curLab = null;
                }
                if (model.prm is UiModAssetSelectVideoWindowParam && !StoryVideoAssetForm.DatasByLab.ContainsKey(model.curLab))
                {
                    model.curLab = null;
                }
            }

            model.sel = null;
            Refresh();
        }
        public void Refresh()
        {
            RefreshLabs();
            RefreshItems();
            view.sta_selected.ChangeState(model.sel != null ? 1 : 0);
            view.sta_setLabel.ChangeState(model.isSetLabelMode ? 1 : 0);
        }
        void RefreshLabs()
        {
            labCon.Clear();
            HashSet<string> labs = new HashSet<string>();
            if (model.prm is UiModAssetSelectTexWindowParam)
            {
                foreach (var data in StoryTexAssetForm.DataById.Values)
                {
                    if (!string.IsNullOrEmpty(data.lab))
                        labs.Add(data.lab);
                }
            }
            else if (model.prm is UiModAssetSelectAudioWindowParam)
            {
                foreach (var data in AudioAssetForm.DataById.Values)
                {
                    if (!string.IsNullOrEmpty(data.lab))
                        labs.Add(data.lab);
                }
            }
            else if (model.prm is UiModAssetSelectVideoWindowParam)
            {
                foreach (var data in VideoAssetForm.DataById.Values)
                {
                    if (!string.IsNullOrEmpty(data.lab))
                        labs.Add(data.lab);
                }
            }
            labCon.Add(new UiLabParam() { lab = null });
            foreach (var lab in labs)
            {
                labCon.Add(new UiLabParam() { lab = lab });
            }
            labCon.Add(new UiLabParam() { lab = TextManager.instance.GetTxt("new") });
            labCon.Refresh();
        }
        void RefreshItems()
        {
            itemCon.Clear();
            if (model.prm is UiModAssetSelectTexWindowParam)
            {
                foreach (var data in StoryTexAssetForm.DataById.Values)
                {
                    if ((model.curLab == null && string.IsNullOrEmpty(data.lab)) || data.lab == model.curLab)
                    {
                        itemCon.Add(new UiItemParam()
                        {
                            data = data
                        });
                    }
                }
            }
            else if (model.prm is UiModAssetSelectAudioWindowParam)
            {
                foreach (var data in AudioAssetForm.DataById.Values)
                {
                    if (model.curLab == null || data.lab == model.curLab)
                    {
                        itemCon.Add(new UiItemParam()
                        {
                            data = data
                        });
                    }
                }
            }
            else if (model.prm is UiModAssetSelectVideoWindowParam)
            {
                foreach (var data in VideoAssetForm.DataById.Values)
                {
                    if (model.curLab == null || data.lab == model.curLab)
                    {
                        itemCon.Add(new UiItemParam()
                        {
                            data = data
                        });
                    }
                }
            }
            itemCon.Add(new UiItemParam()
            {
                data = null
            });
            itemCon.Refresh();
        }
        public void SetCurLab(string lab)
        {
            model.curLab = lab;
            model.sel = null;
            RefreshItems();
        }
        public void Import(string lab)
        {
            if (model.prm is UiModAssetSelectTexWindowParam texPrm)
            {
                AssetManager.instance.texCtrl.Select(texPrm.sizeLimit, (data) =>
                {
                    data.lab = lab;
                    GameManager.instance.saveCtrl.AddStoryTex(data);
                    Refresh();
                });
            }
            else if (model.prm is UiModAssetSelectAudioWindowParam audioPrm)
            {
                AssetManager.instance.audioCtrl.Select((data) =>
                {
                    data.lab = lab;
                    GameManager.instance.saveCtrl.AddStoryAudio(data);
                    Refresh();
                });
            }
            else if (model.prm is UiModAssetSelectVideoWindowParam videoPrm)
            {
                AssetManager.instance.videoCtrl.Select((data) =>
                {
                    data.lab = lab;
                    GameManager.instance.saveCtrl.AddStoryVideo(data);
                    Refresh();
                });
            }
        }

        public void OnEvent(AssetEvent evt)
        {
            Refresh();
        }
    }

    public partial class UiLabParam
    {
        public string lab;
    }
    public partial class UiLabModel
    {
        public UiLabParam prm;
    }
    public partial class UiLabCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (parent.model.isSetLabelMode && parent.model.sel != null)
                {
                    parent.model.sel.lab = model.prm.lab;
                    parent.SetCurLab(model.prm.lab);
                    parent.model.isSetLabelMode = false;
                    parent.Refresh();
                }
                else if (model.prm.lab == TextManager.instance.GetTxt("new"))
                {
                    if(parent.model.sel != null)
                    {
                        NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("inputNewLab"), true, (v) =>
                        {
                            parent.model.sel.lab = v;
                            parent.SetCurLab(model.prm.lab);
                            parent.model.isSetLabelMode = false;
                            parent.Refresh();
                            return true;
                        });
                    }
                }
                else
                {
                    if (parent.model.sel != null && model.prm.lab != null)
                    {
                        parent.model.sel.lab = model.prm.lab;
                        parent.model.sel = null;
                    }
                    else
                    {
                        parent.SetCurLab(model.prm.lab);
                    }
                    parent.Refresh();

                }
            });
        }
        public override void OnShow()
        {
            model.prm = param;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = model.prm.lab;
            view.sta_.ChangeState(parent.model.curLab == model.prm.lab ? 1 : 0);
            view.sta_valid.ChangeState(model.prm.lab == null ? 0 : 1);
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
            view.btn_new.onClick.AddListener(() =>
            {
                parent.Import(parent.model.curLab);
            });
            view.btn_.onClick.AddListener(() =>
            {
                if (parent.model.sel == model.prm.data)
                {
                    parent.model.sel = null;
                }
                else
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
            if (model.prm.data == null)
            {
                view.sta_exist.ChangeState(0);
                view.sta_.ChangeState(0);
            }
            else
            {
                view.sta_exist.ChangeState(1);
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
                view.sta_.ChangeState(parent.model.sel == model.prm.data ? 1 : 0);

            }
        }
    }


}
