using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_Ui;
using Ui.ModStoryEventEditWindow;
using Unity.VisualScripting;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Text;
using Z_Ui.Notify;

namespace Ui.ModStory.ModStoryEvent.ModStoryEventCustom
{

    public partial class UiModStoryEventCustomParam
    {

    }
    public partial class UiModStoryEventCustomModel
    {
        public string cat;
        public EventProgramDataForm.Data data;
    }
    public partial class UiModStoryEventCustomCtrl
    {

        UiScrViewContainer<UiLabCtrl> labCon;
        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {
            labCon = new UiScrViewContainer<UiLabCtrl>(this, view.go_lab, view.scr_lab);
            itemCon = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            view.ipt_lab.onFinishInput += RenameCurrentLab;
            view.btn_deleteLab.onClick.AddListener(DeleteCurrentLab);
            view.btn_edit.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryEventEditWindowCtrl>(new UiModStoryEventEditWindowParam()
                {
                    data = model.data,
                    onClose = () =>
                    {
                        Sel(GetCategory(model.data), model.data);
                    }
                });
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteEvent(model.data.name);
                Sel(model.cat);
            });
        }
        public override void OnShow()
        {
            var categories = GetCategories();
            model.cat = HasUnclassifiedCategory()
                ? string.Empty
                : categories.FirstOrDefault();
            model.data = GetDatas(model.cat).OrderBy(data => data.uid).FirstOrDefault();
            Refresh();
        }

        private static IEnumerable<LabForm.Data> GetEventLabs()
        {
            var labIds = new HashSet<int>();
            foreach (var data in EventProgramDataForm.DataByUid.Values)
            {
                if (LabForm.TryGetData(data.labId, out var lab) && labIds.Add(lab.id))
                    yield return lab;
            }

            if (LabForm.DatasByBelong.TryGetValue(nameof(EventProgramDataForm), out var labs))
            {
                foreach (var lab in labs)
                {
                    if (lab != null && labIds.Add(lab.id))
                        yield return lab;
                }
            }
        }

        private static string GetCategory(EventProgramDataForm.Data data)
        {
            if (data != null && LabForm.TryGetData(data.labId, out var lab))
                return lab.lv1Lab ?? string.Empty;
            return string.Empty;
        }

        private static List<EventProgramDataForm.Data> GetDatas(string category = null)
        {
            var result = new List<EventProgramDataForm.Data>();
            foreach (var data in EventProgramDataForm.DataByUid.Values)
            {
                if (category != null && GetCategory(data) != category)
                    continue;
                result.Add(data);
            }
            return result;
        }
        internal static List<string> GetCategories()
        {
            return GetEventLabs()
                .Where(lab => !string.IsNullOrEmpty(lab.lv1Lab))
                .GroupBy(lab => lab.lv1Lab)
                .OrderBy(group => group.Min(lab => lab.id))
                .Select(group => group.Key)
                .ToList();
        }
        private static bool HasUnclassifiedCategory()
        {
            return GetDatas(string.Empty).Count > 0;
        }
        private bool TryGetCurrentLab(out LabForm.Data lab)
        {
            lab = GetEventLabs().FirstOrDefault(item =>
                item.belong == nameof(EventProgramDataForm) &&
                !string.IsNullOrEmpty(model.cat) &&
                item.lv1Lab == model.cat);
            return lab != null;
        }
        private void RefreshLabEditor()
        {
            var canEdit = TryGetCurrentLab(out _);
            view.ipt_lab.gameObject.SetActive(canEdit);
            view.btn_deleteLab.gameObject.SetActive(canEdit);
            view.ipt_lab.Set(canEdit ? model.cat : string.Empty);
        }
        private void RenameCurrentLab(string value)
        {
            if (!TryGetCurrentLab(out _) || string.IsNullOrWhiteSpace(value))
            {
                Refresh();
                return;
            }

            var oldName = model.cat;
            var newName = value.Trim();
            if (newName == oldName)
            {
                Refresh();
                return;
            }

            // A row represents one first-level name, even when legacy data uses several Lab IDs.
            var oldLabIds = GetEventLabs()
                .Where(lab => lab.belong == nameof(EventProgramDataForm) && lab.lv1Lab == oldName)
                .Select(lab => lab.id)
                .ToList();
            var newLabId = LabForm.GetOrCreate(newName, string.Empty, string.Empty, nameof(EventProgramDataForm));
            foreach (var data in GetDatas(oldName))
                data.labId = newLabId;
            foreach (var oldLabId in oldLabIds)
                LabForm.RemoveData(oldLabId);

            Sel(newName, model.data);
        }
        private void DeleteCurrentLab()
        {
            if (!TryGetCurrentLab(out _))
                return;

            var oldName = model.cat;
            var oldLabIds = GetEventLabs()
                .Where(lab => lab.belong == nameof(EventProgramDataForm) && lab.lv1Lab == oldName)
                .Select(lab => lab.id)
                .ToList();
            foreach (var data in GetDatas(oldName))
                data.labId = LabForm.NoneId;
            foreach (var oldLabId in oldLabIds)
                LabForm.RemoveData(oldLabId);

            Sel(string.Empty);
        }
        public void Refresh()
        {
            labCon.Clear();
            var categories = GetCategories();
            var hasUnclassifiedCategory = HasUnclassifiedCategory();
            if (model.cat != null &&
                ((model.cat == string.Empty && !hasUnclassifiedCategory) ||
                 (model.cat != string.Empty && !categories.Contains(model.cat))))
            {
                model.cat = hasUnclassifiedCategory ? string.Empty : categories.FirstOrDefault();
                model.data = null;
            }
            if (hasUnclassifiedCategory)
            {
                labCon.Add(new UiLabParam()
                {
                    cat = string.Empty
                });
            }
            foreach (var cat in categories)
            {
                labCon.Add(new UiLabParam()
                {
                    cat = cat
                });
            }
            labCon.Add(new UiLabParam()
            {
                isNew = true
            });
            labCon.Refresh();
            RefreshLabEditor();

            itemCon.Clear();

            foreach (var data in GetDatas(model.cat).OrderBy(d => d.uid))
            {
                itemCon.Add(new UiItemParam()
                {
                    data = data
                });
            }
            itemCon.Add(new UiItemParam()
            {
                data = null
            });
            itemCon.Refresh();

            if (model.data != null)
            {
                view.txt_name.text = model.data.name;
                view.txt_desc.text = model.data.code;
            }
            view.go_show.SetActive(model.data != null);

        }
        public void Sel(string cat = null, EventProgramDataForm.Data data = null)
        {
            model.cat = cat;
            model.data = data ?? GetDatas(model.cat).OrderBy(item => item.uid).FirstOrDefault();
            Refresh();
        }

        public void CreateCategory()
        {
            NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("new"), true, value =>
            {
                var category = value?.Trim() ?? string.Empty;
                if (category.Length == 0)
                    return false;

                LabForm.GetOrCreate(category, string.Empty, string.Empty, nameof(EventProgramDataForm));
                Sel(category);
                return true;
            });
        }
    }

    public partial class UiLabParam
    {
        public string cat;
        public bool isNew;
    }
    public partial class UiLabModel
    {
        public string cat;
        public bool isNew;
    }
    public partial class UiLabCtrl
    {

        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (model.isNew)
                    parent.CreateCategory();
                else
                    parent.Sel(model.cat);
            });
        }
        public override void OnShow()
        {
            model.cat = param.cat;
            model.isNew = param.isNew;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = model.isNew
                ? TextManager.instance.GetTxt("new")
                : model.cat == null
                    ? TextManager.instance.GetTxt("all")
                    : string.IsNullOrEmpty(model.cat)
                        ? TextManager.instance.GetTxt(GlobalDefaultHelper.defaultLab)
                        : model.cat;

            if (model.isNew)
            {
                view.sta_state.ChangeState(UiLabRenderHelper.NewState);
                view.sta_.ChangeState(0);
                return;
            }

            view.sta_state.ChangeState(model.cat == null
                ? UiLabRenderHelper.AllState
                : string.IsNullOrEmpty(model.cat)
                    ? UiLabRenderHelper.UnclassifiedState
                    : UiLabRenderHelper.LabState);
            view.sta_.ChangeState(model.cat == parent.model.cat ? 1 : 0);
        }
    }

    public partial class UiItemParam
    {
        public EventProgramDataForm.Data data;
    }
    public partial class UiItemModel
    {
        public EventProgramDataForm.Data data;
    }
    public partial class UiItemCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                if (model.data == null)
                {
                    var labId = LabForm.GetOrCreate(parent.model.cat, string.Empty, string.Empty, nameof(EventProgramDataForm));
                    ModManager.instance.assetCtrl.CreateEvent(labId, "");
                    parent.Refresh();
                }
                else
                {
                    parent.Sel(parent.model.cat, model.data);
                }
            });


        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            bool isNew = model.data == null;
            view.txt_.text = isNew
                ? TextManager.instance.GetTxt("new")
                : model.data.name;

            view.sta_state.ChangeState(isNew
                ? UiLabRenderHelper.NewState
                : UiLabRenderHelper.LabState);
            view.sta_.ChangeState(!isNew && parent.model.data == model.data ? 1 : 0);
        }
    }


}
