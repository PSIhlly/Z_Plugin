using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using UnityEngine.UIElements;
using Z_Text;
using UnityEngine;
using Z_DataSystem.Form;
using Z_UnitSystem;
using Z_Ui.Notify;
using Z_String;
using Z_Math;

namespace Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject.ModStoryMapObjectObjectAppearance
{

    public partial class UiModStoryMapObjectObjectAppearanceParam
    {
        public MapObjectForm.Data data;
    }
    public partial class UiModStoryMapObjectObjectAppearanceModel
    {
        public MapObjectForm.Data data;
        public int id;
    }
    public partial class UiModStoryMapObjectObjectAppearanceCtrl
    {
        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {
            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);
            view.btn_reset.onClick.AddListener(() =>
            {
                model.data.model.subPrefabUnitScale[model.id] = Vector3.one;
                model.data.model.subPrefabUnitPos[model.id] = Vector3.zero;
                Refresh();
            });
            view.btn_model.onClick.AddListener(() =>
            {
                var lst = new List<(string, Sprite)>();
                foreach (var form in GameObjectAssetForm.DataById.Values)
                {
                    if (!form.name.StartsWith(GlobalNameHelper.GetInternalPrefabName("")))
                    {
                        lst.Add((form.name, null));
                    }
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("chooseModel"), false, (res) =>
                {
                    model.data.model.subPrefabUnitName[model.id] = lst[res].Item1;
                    return true;
                }, lst);
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteObject(model.data.name);
                model.id = -1;
                Refresh();
            });
            view.btn_deleteUnit.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteObjectUnit(model.data.name,model.id);
                model.id = -1;
                Refresh();
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                model.data.name = s;
                Refresh();
            };
            view.ipt_height.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[model.id].SetY(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.ipt_length.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[model.id].SetX(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.ipt_width.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[model.id].SetZ(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.data = param.data;
            }
            model.id = -1;
            Refresh();
        }
        public override void Close()
        {
            GameManager.instance.saveCtrl.SaveItem(ModManager.instance.GetStoryCoreFolder());
            base.Close();
        }

        public void Refresh()
        {
            view.sta_show.ChangeState(model.id == -1 ? 0 : 1);

            view.ipt_width.Set(model.data.name);

            if (model.id != -1)
            {
                view.ipt_height.Set(model.data.model.subPrefabUnitScale[model.id].y.ToString("#0.0"));
                view.ipt_length.Set(model.data.model.subPrefabUnitScale[model.id].x.ToString("#0.0"));
                view.ipt_width.Set(model.data.model.subPrefabUnitScale[model.id].z.ToString("#0.0"));

            }
            itemCon.Clear();
            for (int i = 0; i < model.data.model.subPrefabUnitName.Count; i++)
            {
                itemCon.Add(new UiItemParam()
                {
                    id = i,
                });
            }
            itemCon.Add(new UiItemParam()
            {
                id = -1,
            });
            itemCon.Refresh();

        }
    }


    public partial class UiItemParam
    {
        public int id;
    }
    public partial class UiItemModel
    {
        public int id;
    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.id = model.id;
                parent.Refresh();
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.id = param.id;
            }
            Refresh();
        }

        public void Refresh()
        {
            view.sta_item.ChangeState(model.id >= 0 ? 1 : 0);
            if (model.id >= 0)
            {
                view.txt_.text = TextManager.instance.GetTxt(parent.model.data.model.subPrefabUnitName[model.id].ToString());
                view.sta_item.ChangeState(model.id == parent.model.id ? 1 : 0);
            }
        }
    }
}