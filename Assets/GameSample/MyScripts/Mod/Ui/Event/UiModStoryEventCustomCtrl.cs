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
            catCon = new UiScrViewContainer<UiCategoryCtrl>(view.go_category, view.scr_categorys);
            typeCon = new UiScrViewContainer<UiTypeCtrl>(view.go_type, view.scr_types);
            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);
            view.btn_edit.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryEventEditWindowCtrl>(new UiModStoryEventEditWindowParam()
                {
                    data = model.data
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
            model.cat = null ;
            model.type = null;
            model.data = null ;
            Refresh();
        }
        public void Refresh()
        {
            catCon.Clear();
            if (EventProgramDataForm.DatasByCategory.ContainsKey(""))
                catCon.Add(new UiCategoryParam()
                {
                    cat = ""
                });
            foreach (var cat in EventProgramDataForm.DatasByCategory.Keys)
            {
                if (cat != "")
                    catCon.Add(new UiCategoryParam()
                    {
                        cat = cat
                    });
            }
            catCon.Refresh();

            typeCon.Clear();

            if (model.cat != null)
            {
                HashSet<string> exist = new HashSet<string>();
                var lst = EventProgramDataForm.DatasByCategory[model.cat];

                foreach (var data in lst)
                {
                    if (exist.Contains(data.type))
                        continue;
                    exist.Add(data.type);

                }
                if (exist.Contains(""))
                    typeCon.Add(new UiTypeParam()
                    {
                        type = ""
                    });
                foreach (var type in exist)
                {
                    if(type!="")
                    typeCon.Add(new UiTypeParam()
                    {
                        type = type
                    });
                }
            }
            typeCon.Refresh();

            itemCon.Clear();

            if (model.type != null)
            {
                foreach (var data in EventProgramDataForm.DatasByCategoryType[(model.cat, model.type)])
                {
                    itemCon.Add(new UiItemParam()
                    {
                        data = data
                    });
                }
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
                view.txt_desc.text = model.data.code;
            }

        }
        public void Sel(string cat = null, string type = null, EventProgramDataForm.Data data = null)
        {

            if (cat == null || !EventProgramDataForm.DatasByCategory.ContainsKey(cat))
            {
                cat = null;
                type = null;
                data = null;
            }
            else if (!EventProgramDataForm.DatasByCategoryType.ContainsKey((cat, type)))
            {
                type = null;
                data = null;
            }
            model.cat = cat;
            model.type = type;
            model.data = data;
            Refresh();
        }
    }

    public partial class UiCategoryParam
    {
        public string cat;
    }
    public partial class UiCategoryModel
    {
        public string cat;
    }
    public partial class UiCategoryCtrl
    {

        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.Sel(model.cat);
            });
        }
        public override void OnShow()
        {
            model.cat =param.cat;
            Refresh();
        }
        public void Refresh()
        {

            view.sta_valid.ChangeState(model.cat == "" ? 0 : 1);
            if (model.cat != null)
            {
                view.sta_.ChangeState(model.cat == parent.model.cat ? 1 : 0);
                view.txt_.text = model.cat;
            }
        }
    }

    public partial class UiTypeParam
    {
        public string type;
    }
    public partial class UiTypeModel
    {
        public string type;
    }
    public partial class UiTypeCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.Sel(parent.model.cat, model.type);
            });

        }
        public override void OnShow()
        {
            model.type =param.type;
            Refresh();
        }
        public void Refresh()
        {

            view.sta_valid.ChangeState(model.type == "" ? 0 : 1);
            if (model.type != null)
            {
                view.sta_.ChangeState(model.type == parent.model.type ? 1 : 0);
                view.txt_.text = model.type;
            }
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
                    ModManager.instance.assetCtrl.CreateEvent("", parent.model.cat, parent.model.type);
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
            view.sta_valid.ChangeState(model.data == null ? 0 : 1);
            if (model.data != null)
            {
                view.sta_.ChangeState(parent.model.data == model.data ? 1 : 0);
                view.txt_.text = model.data.name;
            }
        }
    }


}