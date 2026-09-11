using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Unity.VisualScripting;
using Z_DataSystem.Form;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectList
{

    public partial class UiModStoryMapObjectListParam
    {
        public int type;
        public int? labId;
    }
    public partial class UiModStoryMapObjectListModel
    {
        public Dictionary<int, List<MapBaseForm.Data>> datas;
        public Action<int> createAct;
        public int? labId;
    }
    public partial class UiModStoryMapObjectListCtrl
    {

        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiBigItemCtrl> itemCon;
        public override void OnCreate()
        {
            model.datas = new Dictionary<int, List<MapBaseForm.Data>>();
            labCon = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_labs);
            itemCon = new UiScrViewContainer<UiBigItemCtrl>(this, view.go_bigItem, view.scr_bigItems);
            view.ipt_lab.onFinishInput += RenameCurrentLab;
            view.btn_deleteLab.onClick.AddListener(DeleteCurrentLab);
            view.btn_back.onClick.AddListener(() =>
            {
                parent.SelType(0);
            });
        }
        public override void OnShow()
        {
            Update();
            model.labId = HasUnclassified() ? LabForm.NoneId : (int?)null;
            Refresh();
        }
        bool HasUnclassified()
        {
            return model.datas.ContainsKey(LabForm.NoneId);
        }
        public void Update()
        {
            model.datas.Clear();
            switch (param.type)
            {
                case 1:
                    foreach (var pair in MapTextureForm.DatasByLabid)
                    {
                        if (!model.datas.TryGetValue(pair.Key, out var datas))
                        {
                            datas = new List<MapBaseForm.Data>();
                            model.datas[pair.Key] = datas;
                        }
                        foreach (var data in pair.Value)
                        {
                            datas.Add(data);
                        }
                    }
                    model.createAct = (labId) =>
                    {
                        ModManager.instance.assetCtrl.CreateTex(labId);
                    };
                    break;
                case 2:
                    foreach (var pair in MapMaskForm.DatasByLabid)
                    {
                        if (!model.datas.TryGetValue(pair.Key, out var datas))
                        {
                            datas = new List<MapBaseForm.Data>();
                            model.datas[pair.Key] = datas;
                        }
                        foreach (var data in pair.Value)
                        {
                            datas.Add(data);
                        }
                    }
                    model.createAct = (labId) =>
                    {
                        ModManager.instance.assetCtrl.CreateMask(labId);
                    };
                    break;
                case 3:
                    foreach (var pair in MapObjectForm.DatasByLabid)
                    {
                        if (!model.datas.TryGetValue(pair.Key, out var datas))
                        {
                            datas = new List<MapBaseForm.Data>();
                            model.datas[pair.Key] = datas;
                        }
                        foreach (var data in pair.Value)
                        {
                            datas.Add(data);
                        }
                    }
                    model.createAct = (labId) =>
                    {
                        ModManager.instance.assetCtrl.CreateObject(labId);
                    };
                    break;

            }
        }
        public void Refresh()
        {

            var hasUnclassified = HasUnclassified();
            if (model.labId == LabForm.NoneId && !hasUnclassified)
                model.labId = null;
            labCon.Clear();
            labCon.Add(new UiLabParam()
            {
                labId = null
            });
            if (hasUnclassified)
            {
                labCon.Add(new UiLabParam()
                {
                    labId = LabForm.NoneId
                });
            }
            foreach (var labId in UiLabRenderHelper.GetLabIds(GetLabBelong()))
            {
                labCon.Add(new UiLabParam()
                {
                    labId = labId
                });
            }
            labCon.Add(new UiLabParam()
            {
                isNew = true
            });
            labCon.Refresh();
            RefreshLabEditor();
            itemCon.Clear();

            List<MapBaseForm.Data> datas = null;
            if (!model.labId.HasValue)
            {
                datas=new List<MapBaseForm.Data>();
                foreach(var v in model.datas.Values)
                {
                    datas.AddRange(v);
                }
            }
            else
            {
                if (!model.datas.TryGetValue(model.labId.Value, out datas))
                {
                    datas = new List<MapBaseForm.Data>();
                }
            }

            foreach (var data in datas.OrderBy(d => d.id))
            {
                itemCon.Add(new UiBigItemParam()
                {
                    data = data
                });
            }
            itemCon.Add(new UiBigItemParam()
            {
                data = null
            });
            itemCon.Refresh();



        }

        bool TryGetCurrentLab(out LabForm.Data lab)
        {
            lab = null;
            return model.labId.HasValue &&
                   model.labId.Value != LabForm.NoneId &&
                   LabForm.TryGetData(model.labId.Value, out lab) &&
                   lab.belong == GetLabBelong();
        }

        void RefreshLabEditor()
        {
            var canEdit = TryGetCurrentLab(out _);
            view.ipt_lab.gameObject.SetActive(canEdit);
            view.btn_deleteLab.gameObject.SetActive(canEdit);
            view.ipt_lab.Set(canEdit ? LabForm.GetDisplayName(model.labId.Value) : "");
        }

        void RenameCurrentLab(string value)
        {
            if (!TryGetCurrentLab(out var currentLab) || string.IsNullOrWhiteSpace(value))
            {
                Refresh();
                return;
            }

            var oldLabId = currentLab.id;
            var newLabId = LabForm.GetOrCreateDisplayName(value.Trim(), GetLabBelong(), oldLabId);
            if (newLabId == LabForm.NoneId)
            {
                Refresh();
                return;
            }

            if (newLabId != oldLabId)
            {
                ReassignLab(oldLabId, newLabId);
                LabForm.RemoveData(oldLabId);
                model.labId = newLabId;
                Update();
            }

            Refresh();
        }

        void DeleteCurrentLab()
        {
            if (!TryGetCurrentLab(out var currentLab))
                return;

            ReassignLab(currentLab.id, LabForm.NoneId);
            LabForm.RemoveData(currentLab.id);
            Update();
            model.labId = HasUnclassified() ? LabForm.NoneId : (int?)null;
            Refresh();
        }

        void ReassignLab(int oldLabId, int newLabId)
        {
            switch (param.type)
            {
                case 1:
                    foreach (var data in MapTextureForm.DataById.Values.ToList())
                    {
                        if (data.labId == oldLabId)
                            data.labId = newLabId;
                    }
                    break;
                case 2:
                    foreach (var data in MapMaskForm.DataById.Values.ToList())
                    {
                        if (data.labId == oldLabId)
                            data.labId = newLabId;
                    }
                    break;
                case 3:
                    foreach (var data in MapObjectForm.DataById.Values.ToList())
                    {
                        if (data.labId == oldLabId)
                            data.labId = newLabId;
                    }
                    break;
            }
        }

        public string GetLabBelong()
        {
            switch (param.type)
            {
                case 1:
                    return nameof(MapTextureForm);
                case 2:
                    return nameof(MapMaskForm);
                case 3:
                    return nameof(MapObjectForm);
                default:
                    return string.Empty;
            }
        }
    }

    public partial class UiLabParam
    {
        public int? labId;
        public bool isNew;
    }
    public partial class UiLabModel
    {
        public int? labId;
        public bool isNew;
    }
    public partial class UiLabCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                if (model.isNew)
                {
                    UiLabRenderHelper.Create(parent.GetLabBelong(), labId =>
                    {
                        parent.model.labId = labId;
                        parent.Refresh();
                    });
                }
                else
                {
                    parent.model.labId = model.labId;
                    parent.Refresh();
                }
            });

        }
        public override void OnShow()
        {
            model.labId = param.labId;
            model.isNew = param.isNew;
            Refresh();
        }
        public void Refresh()
        {

            view.sta_state.ChangeState(UiLabRenderHelper.GetState(model.labId, model.isNew));
            view.sta_.ChangeState(!model.isNew && model.labId == parent.model.labId ? 1 : 0);
            view.txt_.text = UiLabRenderHelper.GetText(model.labId, model.isNew);
        }
    }


    public partial class UiBigItemParam
    {
        public MapBaseForm.Data data;
    }
    public partial class UiBigItemModel
    {
        public MapBaseForm.Data data;
    }
    public partial class UiBigItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                parent.model.createAct?.Invoke(parent.model.labId ?? LabForm.NoneId);
                parent.Update();
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelData(model.data);
            });

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null)
            {
                view.txt_.text = model.data.name;
                int texId = GetFirstAnimTexId(model.data);
                if (texId != 0 && TexAssetForm.DataById.ContainsKey(texId))
                    view.img_.BindTexData(TexAssetForm.DataById[texId]);
            }
        }
        /// <summary>
        /// 获取data动画帧的第一张贴图id，替代icon字段
        /// </summary>
        private int GetFirstAnimTexId(MapBaseForm.Data data)
        {
            if (data is MapTextureForm.Data texData && texData.texs != null && texData.texs.Count > 0)
                return texData.texs[0];
            if (data is MapMaskForm.Data maskData && maskData.texsName != null && maskData.texsName.Count > 0)
                return maskData.texsName[0];
            if (data is MapObjectForm.Data objData)
            {
                var clip = objData.GetAnimClip(objData.GetDefaultAnimDirection());
                if (clip.Count > 0)
                    return clip[0];
            }
            return 0;
        }
    }


}
