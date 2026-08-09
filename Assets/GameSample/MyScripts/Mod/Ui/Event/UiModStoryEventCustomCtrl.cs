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
        public string type;
        public EventProgramDataForm.Data data;
    }
    public partial class UiModStoryEventCustomCtrl
    {

        UiScrViewContainer<UiCategoryCtrl> catCon;
        UiScrViewContainer<UiTypeCtrl> typeCon;
        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {
            catCon = new UiScrViewContainer<UiCategoryCtrl>(this, view.go_category, view.scr_categorys);
            typeCon = new UiScrViewContainer<UiTypeCtrl>(this, view.go_type, view.scr_types);
            itemCon = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            view.btn_edit.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryEventEditWindowCtrl>(new UiModStoryEventEditWindowParam()
                {
                    data = model.data,
                    onClose = () =>
                    {
                        Refresh();
                    }
                });
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteEvent(model.data.name);
                Sel(model.cat, model.type);
            });
        }
        public override void OnShow()
        {
            var categories = GetCategories();
            model.cat = HasUnclassifiedCategory()
                ? string.Empty
                : categories.FirstOrDefault();
            model.type = model.cat != null && HasUnclassifiedType(model.cat)
                ? string.Empty
                : null;
            model.data = GetDatas(model.cat, model.type).OrderBy(data => data.uid).FirstOrDefault();
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

        private static void GetLabLevels(EventProgramDataForm.Data data, out string category, out string type)
        {
            category = string.Empty;
            type = string.Empty;
            if (data != null && LabForm.TryGetData(data.labId, out var lab))
            {
                category = lab.lv1Lab ?? string.Empty;
                type = lab.lv2Lab ?? string.Empty;
            }
        }

        private static List<EventProgramDataForm.Data> GetDatas(string category = null, string type = null)
        {
            var result = new List<EventProgramDataForm.Data>();
            foreach (var data in EventProgramDataForm.DataByUid.Values)
            {
                GetLabLevels(data, out var dataCategory, out var dataType);
                if (category != null && dataCategory != category)
                    continue;
                if (type != null && dataType != type)
                    continue;
                result.Add(data);
            }
            return result;
        }
        private static List<string> GetCategories()
        {
            return GetEventLabs()
                .Select(lab => lab.lv1Lab ?? string.Empty)
                .Where(category => !string.IsNullOrEmpty(category))
                .Distinct()
                .OrderBy(category => category, StringComparer.Ordinal)
                .ToList();
        }
        private static List<string> GetTypes(string category)
        {
            return GetEventLabs()
                .Where(lab => (lab.lv1Lab ?? string.Empty) == category)
                .Select(lab => lab.lv2Lab ?? string.Empty)
                .Where(type => !string.IsNullOrEmpty(type))
                .Distinct()
                .OrderBy(type => type, StringComparer.Ordinal)
                .ToList();
        }
        private static bool HasUnclassifiedCategory()
        {
            return GetDatas(string.Empty).Count > 0;
        }
        private static bool HasUnclassifiedType(string category)
        {
            return GetDatas(category, string.Empty).Count > 0;
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
                model.type = null;
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

            typeCon.Clear();

            if (model.cat != null)
            {
                var types = GetTypes(model.cat);
                var hasUnclassified = HasUnclassifiedType(model.cat);
                if (model.type == string.Empty && !hasUnclassified)
                    model.type = null;
                typeCon.Add(new UiTypeParam()
                {
                    type = null
                });
                if (hasUnclassified)
                {
                    typeCon.Add(new UiTypeParam()
                    {
                        type = string.Empty
                    });
                }
                foreach (var type in types)
                {
                    typeCon.Add(new UiTypeParam()
                    {
                        type = type
                    });
                }
                typeCon.Add(new UiTypeParam()
                {
                    isNew = true
                });
            }
            typeCon.Refresh();

            itemCon.Clear();

            foreach (var data in GetDatas(model.cat, model.type).OrderBy(d => d.uid))
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

            view.go_show.SetActive(model.data != null);
            if (model.data != null)
            {
                view.txt_name.text = model.data.name;
                view.txt_desc.oriText=model.data.code;
            }

        }
        public void Sel(string cat = null, string type = null, EventProgramDataForm.Data data = null)
        {
            model.cat = cat;
            model.type = cat == null ? null : type;
            model.data = data ?? GetDatas(model.cat, model.type).OrderBy(item => item.uid).FirstOrDefault();
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
                Sel(category, null);
                return true;
            });
        }

        public void CreateType()
        {
            if (model.cat == null)
                return;

            NotifyManager.instance.AddInputArea(TextManager.instance.GetTxt("new"), true, value =>
            {
                var type = value?.Trim() ?? string.Empty;
                if (type.Length == 0)
                    return false;

                LabForm.GetOrCreate(model.cat, type, string.Empty, nameof(EventProgramDataForm));
                Sel(model.cat, type);
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

            if (model.isNew)
            {
                view.sta_state.ChangeState(UiLabRenderHelper.NewState);
                view.sta_.ChangeState(0);
                view.txt_.text = TextManager.instance.GetTxt("new");
                return;
            }

            view.sta_state.ChangeState(model.cat == null
                ? UiLabRenderHelper.AllState
                : string.IsNullOrEmpty(model.cat)
                    ? UiLabRenderHelper.UnclassifiedState
                    : UiLabRenderHelper.LabState);
            view.sta_.ChangeState(model.cat == parent.model.cat ? 1 : 0);
            view.txt_.text = model.cat == null
                ? TextManager.instance.GetTxt("all")
                : string.IsNullOrEmpty(model.cat)
                    ? TextManager.instance.GetTxt(GlobalDefaultHelper.defaultLab)
                    : model.cat;
        }
    }

    public partial class UiTypeParam
    {
        public string type;
        public bool isNew;
    }
    public partial class UiTypeModel
    {
        public string type;
        public bool isNew;
    }
    public partial class UiTypeCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                if (model.isNew)
                    parent.CreateType();
                else
                    parent.Sel(parent.model.cat, model.type);
            });

        }
        public override void OnShow()
        {
            model.type = param.type;
            model.isNew = param.isNew;
            Refresh();
        }
        public void Refresh()
        {

            if (model.isNew)
            {
                view.sta_state.ChangeState(UiLabRenderHelper.NewState);
                view.sta_.ChangeState(0);
                view.txt_.text = TextManager.instance.GetTxt("new");
                return;
            }

            view.sta_state.ChangeState(model.type == null
                ? UiLabRenderHelper.AllState
                : string.IsNullOrEmpty(model.type)
                    ? UiLabRenderHelper.UnclassifiedState
                    : UiLabRenderHelper.LabState);
            view.sta_.ChangeState(model.type == parent.model.type ? 1 : 0);
            view.txt_.text = model.type == null
                ? TextManager.instance.GetTxt("all")
                : string.IsNullOrEmpty(model.type)
                    ? TextManager.instance.GetTxt(GlobalDefaultHelper.defaultLab)
                    : model.type;
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
                    var labId = LabForm.GetOrCreate(parent.model.cat, parent.model.type, string.Empty, nameof(EventProgramDataForm));
                    ModManager.instance.assetCtrl.CreateEvent(labId, "");
                    parent.Refresh();
                }
                else
                {
                    parent.Sel(parent.model.cat, parent.model.type, model.data);
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
            view.sta_state.ChangeState(model.data == null
                ? UiLabRenderHelper.NewState
                : UiLabRenderHelper.LabState);
            if (model.data != null)
            {
                view.sta_.ChangeState(parent.model.data == model.data ? 1 : 0);
                view.txt_.text = model.data.name;
            }
        }
    }


}
