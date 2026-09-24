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

        UiScrViewContainer<UiCategoryCtrl> catCon;
        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {
            catCon = new UiScrViewContainer<UiCategoryCtrl>(this, view.go_category, view.scr_categorys);
            itemCon = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            var itemRect = (RectTransform)view.scr_items.transform;
            var typeRect = (RectTransform)view.scr_types.transform;
            itemRect.anchorMin = new Vector2(typeRect.anchorMin.x, itemRect.anchorMin.y);
            view.scr_types.gameObject.SetActive(false);
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
                .Select(lab => lab.lv1Lab ?? string.Empty)
                .Where(category => !string.IsNullOrEmpty(category))
                .Distinct()
                .OrderBy(category => category, StringComparer.Ordinal)
                .ToList();
        }
        private static bool HasUnclassifiedCategory()
        {
            return GetDatas(string.Empty).Count > 0;
        }
        public void Refresh()
        {
            catCon.Clear();
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
                catCon.Add(new UiCategoryParam()
                {
                    cat = string.Empty
                });
            }
            foreach (var cat in categories)
            {
                catCon.Add(new UiCategoryParam()
                {
                    cat = cat
                });
            }
            catCon.Add(new UiCategoryParam()
            {
                isNew = true
            });
            catCon.Refresh();

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

    public partial class UiCategoryParam
    {
        public string cat;
        public bool isNew;
    }
    public partial class UiCategoryModel
    {
        public string cat;
        public bool isNew;
    }
    public partial class UiCategoryCtrl
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
