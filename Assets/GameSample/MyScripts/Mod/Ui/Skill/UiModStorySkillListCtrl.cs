using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_DesignStyle;
using Z_DataSystem.Form;
using Z_Text;

namespace Ui.ModStory.ModStorySkill.ModStorySkillList
{

    public partial class UiModStorySkillListParam
    {

    }
    public partial class UiModStorySkillListModel
    {
        public int? labId;
        public SkillProductForm.Data data;
    }
    public partial class UiModStorySkillListCtrl
    {

        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiBigItemCtrl> itemCon;
        public override void OnCreate()
        {

            labCon = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_labs);
            itemCon = new UiScrViewContainer<UiBigItemCtrl>(this, view.go_bigItem, view.scr_bigItems);
            view.ipt_lab.onFinishInput += RenameCurrentLab;
            view.btn_deleteLab.onClick.AddListener(DeleteCurrentLab);

        }
        public override void OnShow()
        {
            model.labId = HasUnclassified() ? LabForm.NoneId : (int?)null;
            Refresh();
        }
        bool HasUnclassified()
        {
            return SkillProductForm.DatasByLabid.ContainsKey(LabForm.NoneId);
        }
        public void Refresh()
        {

            var hasUnclassified = HasUnclassified();
            if (model.labId == LabForm.NoneId && !hasUnclassified)
                model.labId = null;
            labCon.Clear();
            labCon.Add(new UiLabParam()
            {
                state = UiLabRenderHelper.AllState
            });
            if (hasUnclassified)
            {
                labCon.Add(new UiLabParam()
                {
                    labId = LabForm.NoneId,
                    state = UiLabRenderHelper.UnclassifiedState
                });
            }
            foreach (var labId in UiLabRenderHelper.GetLabIds(nameof(SkillProductForm)))
            {
                labCon.Add(new UiLabParam()
                {
                    labId = labId,
                    state = UiLabRenderHelper.LabState
                });
            }
            labCon.Add(new UiLabParam()
            {
                state = UiLabRenderHelper.NewState
            });
            labCon.Refresh();
            RefreshLabEditor();
            itemCon.Clear();
            var datas = model.labId == null
                ? new List<SkillProductForm.Data>(SkillProductForm.DataByUid.Values)
                : (SkillProductForm.DatasByLabid.ContainsKey(model.labId.Value)
                    ? SkillProductForm.DatasByLabid[model.labId.Value]
                    : new List<SkillProductForm.Data>());

            foreach (var data in datas.OrderBy(d => d.uid))
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
                   lab.belong == nameof(SkillProductForm);
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
            var newLabId = LabForm.GetOrCreateDisplayName(value.Trim(), nameof(SkillProductForm), oldLabId);
            if (newLabId == LabForm.NoneId)
            {
                Refresh();
                return;
            }

            if (newLabId != oldLabId)
            {
                foreach (var data in SkillProductForm.DataByUid.Values.ToList())
                {
                    if (data.labId == oldLabId)
                        data.labId = newLabId;
                }
                LabForm.RemoveData(oldLabId);
                model.labId = newLabId;
            }

            Refresh();
        }

        void DeleteCurrentLab()
        {
            if (!TryGetCurrentLab(out var currentLab))
                return;

            var oldLabId = currentLab.id;
            foreach (var data in SkillProductForm.DataByUid.Values.ToList())
            {
                if (data.labId == oldLabId)
                    data.labId = LabForm.NoneId;
            }
            LabForm.RemoveData(oldLabId);
            model.labId = HasUnclassified() ? LabForm.NoneId : (int?)null;
            Refresh();
        }
    }

    public partial class UiLabParam
    {
        public int? labId;
        public int state;
    }
    public partial class UiLabModel
    {
        public int? labId;
        public int state;
    }
    public partial class UiLabCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                if (model.state == UiLabRenderHelper.NewState)
                {
                    UiLabRenderHelper.Create(nameof(SkillProductForm), labId =>
                    {
                        parent.model.labId = labId;
                        parent.Refresh();
                    });
                    return;
                }

                parent.model.labId = model.labId;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.labId = param.labId;
            model.state = param.state;
            Refresh();
        }
        public void Refresh()
        {
            var isNew = model.state == UiLabRenderHelper.NewState;
            view.sta_state.ChangeState(model.state);
            view.sta_.ChangeState(!isNew && model.labId == parent.model.labId ? 1 : 0);
            view.txt_.text = UiLabRenderHelper.GetText(model.labId, isNew);
        }
    }


    public partial class UiBigItemParam
    {
        public SkillProductForm.Data data;
    }
    public partial class UiBigItemModel
    {
        public SkillProductForm.Data data;
    }
    public partial class UiBigItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateSkill(parent.model.labId ?? 0);
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.parent.SelPage(1, model.data);
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
                var tex = TexAssetForm.DataById.GetDv(model.data.icon, null)
                    ?? TexAssetForm.DataById.GetDv(GlobalDefaultHelper.DefaultTexId, null);
                view.img_.BindTexData(tex);
            }
        }
    }


}
