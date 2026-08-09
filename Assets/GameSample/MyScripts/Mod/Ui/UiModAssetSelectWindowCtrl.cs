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
        public int? curLabId;
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
                    ShowSetLabelChoose();
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                if (model.sel != null)
                {
                    model.sel.name = s;
                    Refresh();
                }
            };

            itemCon = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            labCon = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_labs);

        }
        public override void OnShow()
        {
            model.prm = param;
            model.curLabId = HasVisibleUnclassified() ? LabForm.NoneId : (int?)null;

            model.sel = null;
            Refresh();
        }
        public void Refresh()
        {
            RefreshLabs();
            RefreshItems();
            view.sta_selected.ChangeState(model.sel != null ? 1 : 0);
            view.sta_setLabel.ChangeState(0);
            view.ipt_name.Set(model.sel!=null ? model.sel.name : "");
        }
        void RefreshLabs()
        {
            labCon.Clear();
            var labIds = GetVisibleLabIds();
            var hasUnclassified = HasVisibleUnclassified();
            if (model.curLabId == LabForm.NoneId && !hasUnclassified)
                model.curLabId = null;
            labCon.Add(new UiLabParam() { labId = null });
            if (hasUnclassified)
                labCon.Add(new UiLabParam() { labId = LabForm.NoneId });
            foreach (var labId in labIds
                         .OrderBy(id => LabForm.GetDisplayName(id), StringComparer.Ordinal)
                         .ThenBy(id => id))
            {
                labCon.Add(new UiLabParam() { labId = labId });
            }
            labCon.Add(new UiLabParam() { isNew = true });
            labCon.Refresh();
        }

        HashSet<int> GetVisibleLabIds()
        {
            var labIds = new HashSet<int>();
            if (model.prm is UiModAssetSelectTexWindowParam)
            {
                labIds.UnionWith(UiLabRenderHelper.GetLabIds(nameof(StoryTexAssetForm)));
            }
            else if (model.prm is UiModAssetSelectAudioWindowParam)
            {
                labIds.UnionWith(UiLabRenderHelper.GetLabIds(nameof(AudioAssetForm)));
                labIds.UnionWith(UiLabRenderHelper.GetLabIds(nameof(StoryAudioAssetForm)));
            }
            else if (model.prm is UiModAssetSelectVideoWindowParam)
            {
                labIds.UnionWith(UiLabRenderHelper.GetLabIds(nameof(VideoAssetForm)));
                labIds.UnionWith(UiLabRenderHelper.GetLabIds(nameof(StoryVideoAssetForm)));
            }
            return labIds;
        }
        bool HasVisibleUnclassified()
        {
            if (model.prm is UiModAssetSelectTexWindowParam)
                return StoryTexAssetForm.DataById.Values.Any(data => data.labId == LabForm.NoneId);
            if (model.prm is UiModAssetSelectAudioWindowParam)
                return AudioAssetForm.DataById.Values.Any(data => data.labId == LabForm.NoneId);
            if (model.prm is UiModAssetSelectVideoWindowParam)
                return VideoAssetForm.DataById.Values.Any(data => data.labId == LabForm.NoneId);
            return false;
        }
        void RefreshItems()
        {
            itemCon.Clear();
            if (model.prm is UiModAssetSelectTexWindowParam)
            {
                foreach (var data in StoryTexAssetForm.DataById.Values.OrderBy(d => d.id))
                {
                    if (!model.curLabId.HasValue || data.labId == model.curLabId.Value)
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
                foreach (var data in AudioAssetForm.DataById.Values.OrderBy(d => d.id))
                {
                    if (!model.curLabId.HasValue || data.labId == model.curLabId.Value)
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
                foreach (var data in VideoAssetForm.DataById.Values.OrderBy(d => d.id))
                {
                    if (!model.curLabId.HasValue || data.labId == model.curLabId.Value)
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
        public void SetCurLab(int? labId)
        {
            model.curLabId = labId;
            model.sel = null;
            RefreshItems();
        }
        public void Replace()
        {
            if (model.prm is UiModAssetSelectTexWindowParam texPrm)
            {
                AssetManager.instance.texCtrl.Select(texPrm.sizeLimit, (data) =>
                {
                    model.sel.bytes = data.bytes;
                    Refresh();
                });
            }
            else if (model.prm is UiModAssetSelectAudioWindowParam audioPrm)
            {
                AssetManager.instance.audioCtrl.Select((data) =>
                {
                    model.sel.bytes = data.bytes;
                    Refresh();
                });
            }
            else if (model.prm is UiModAssetSelectVideoWindowParam videoPrm)
            {
                AssetManager.instance.videoCtrl.Select((data) =>
                {
                    model.sel.bytes = data.bytes;
                    Refresh();
                });
            }
        }
        public void Import(int labId)
        {
            if (model.prm is UiModAssetSelectTexWindowParam texPrm)
            {
                AssetManager.instance.texCtrl.Select(texPrm.sizeLimit, (data) =>
                {
                    var storyLabId = LabForm.GetOrCreateForBelong(labId, nameof(StoryTexAssetForm));
                    data.labId = storyLabId;
                    GameManager.instance.saveCtrl.AddStoryTex(ref data);
                    SetCurLab(storyLabId == LabForm.NoneId ? (int?)null : storyLabId);
                    Refresh();
                });
            }
            else if (model.prm is UiModAssetSelectAudioWindowParam audioPrm)
            {
                AssetManager.instance.audioCtrl.Select((data) =>
                {
                    var storyLabId = LabForm.GetOrCreateForBelong(labId, nameof(StoryAudioAssetForm));
                    data.labId = storyLabId;
                    GameManager.instance.saveCtrl.AddStoryAudio(data);
                    SetCurLab(storyLabId == LabForm.NoneId ? (int?)null : storyLabId);
                    Refresh();
                });
            }
            else if (model.prm is UiModAssetSelectVideoWindowParam videoPrm)
            {
                AssetManager.instance.videoCtrl.Select((data) =>
                {
                    var storyLabId = LabForm.GetOrCreateForBelong(labId, nameof(StoryVideoAssetForm));
                    data.labId = storyLabId;
                    GameManager.instance.saveCtrl.AddStoryVideo(data);
                    SetCurLab(storyLabId == LabForm.NoneId ? (int?)null : storyLabId);
                    Refresh();
                });
            }
        }
        public void ShowSetLabelChoose()
        {
            var selected = model.sel;
            if (selected == null)
                return;

            var targetBelong = GetLabBelong(selected);
            var items = new EntryItem();
            var unclassifiedText = TextManager.instance.GetTxt(GlobalDefaultHelper.defaultLab);
            items.Add(unclassifiedText, null, LabForm.NoneId);

            var displayNames = new HashSet<string>(StringComparer.Ordinal) { unclassifiedText };
            foreach (var labId in GetVisibleLabIds()
                         .OrderBy(id => LabForm.GetDisplayName(id), StringComparer.Ordinal)
                         .ThenBy(id => id))
            {
                var displayName = LabForm.GetDisplayName(labId);
                if (string.IsNullOrWhiteSpace(displayName) || !displayNames.Add(displayName))
                    continue;
                items.Add(displayName, null, labId);
            }

            NotifyManager.instance.AddChoose(
                TextManager.instance.GetTxt("label"),
                true,
                item =>
                {
                    selected.labId = item.id == LabForm.NoneId
                        ? LabForm.NoneId
                        : LabForm.GetOrCreateForBelong(item.id, targetBelong);
                    Refresh();
                    return true;
                },
                items);
        }

        public string GetLabBelong(AssetForm.Data data)
        {
            var declaringType = data?.GetType().DeclaringType;
            if (declaringType != null)
                return declaringType.Name;
            return GetDefaultLabBelong();
        }

        public string GetDefaultLabBelong()
        {
            if (model.prm is UiModAssetSelectTexWindowParam)
                return nameof(StoryTexAssetForm);
            if (model.prm is UiModAssetSelectAudioWindowParam)
                return nameof(StoryAudioAssetForm);
            if (model.prm is UiModAssetSelectVideoWindowParam)
                return nameof(StoryVideoAssetForm);
            return string.Empty;
        }

        public void OnEvent(AssetEvent evt)
        {
            Refresh();
        }
    }

    public partial class UiLabParam
    {
        public int? labId;
        public bool isNew;
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
                if (model.prm.isNew)
                {
                    UiLabRenderHelper.Create(parent.GetDefaultLabBelong(), labId =>
                    {
                        parent.SetCurLab(labId);
                        parent.Refresh();
                    });
                }
                else
                {
                    parent.SetCurLab(model.prm.labId);
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
            view.txt_.text = UiLabRenderHelper.GetText(model.prm.labId, model.prm.isNew);
            view.sta_.ChangeState(!model.prm.isNew && parent.model.curLabId == model.prm.labId ? 1 : 0);
            view.sta_state.ChangeState(UiLabRenderHelper.GetState(model.prm.labId, model.prm.isNew));
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
                parent.Import(parent.model.curLabId ?? LabForm.NoneId);
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
                    view.img_.BindTexData(((TexAssetForm.Data)model.prm.data));
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
                view.sta_.ChangeState(parent.model.sel == model.prm.data ? 1 : 0);

            }
        }
    }


}
