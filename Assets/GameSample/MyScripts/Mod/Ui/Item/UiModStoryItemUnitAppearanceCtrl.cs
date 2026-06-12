using Form;
using System;
using Ui.Axis;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Math;
using Z_String;
using Z_Text;
using Z_Ui.Base;

namespace Ui.ModStory.ModStoryItem.ModStoryItemUnit.ModStoryItemUnitAppearance
{

    public partial class UiModStoryItemUnitAppearanceParam
    {
        public ItemProductForm.Data data;
    }
    public partial class UiModStoryItemUnitAppearanceModel
    {
        public ItemProductForm.Data data;
        public int id;
    }
    public partial class UiModStoryItemUnitAppearanceCtrl : IZ_Listener<AssetEvent>
    {
        UiScrViewContainer<UiStyleCtrl> styleCon;
        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            styleCon = new UiScrViewContainer<UiStyleCtrl>(this, view.go_style, view.scr_styles);
            itemCon = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            view.btn_reset.onClick.AddListener(() =>
            {
                model.data.model.subPrefabUnitScale[0] = Vector3.one;
                model.data.model.subPrefabUnitPos[0] = Vector3.zero;
                Refresh();
            });
            view.btn_model.onClick.AddListener(() => {
                ModManager.instance.assetCtrl.ChooseModel(TextManager.instance.GetTxt("chooseModel"), (item) =>
                {
                    model.data.model.subPrefabUnitName[0] = item.id;
                    Refresh();
                });
            });

            view.btn_deleteUnit.onClick.AddListener(() => {
                if (model.id >= 0)
                {
                    ModManager.instance.assetCtrl.DeleteItemModelUnitTex(model.data.uid, model.id);
                    model.id = -1;
                    Refresh();
                }
            });
            view.ipt_height.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[0]=model.data.model.subPrefabUnitScale[0].NewSetY(StringHelper.ToFloat(s,1,true));
                Refresh();
            };
            view.ipt_length.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[0]=model.data.model.subPrefabUnitScale[0].NewSetZ(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.ipt_width.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[0] = model.data.model.subPrefabUnitScale[0].NewSetX(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportItemModelUnitTex(model.data.uid, model.id);
                Refresh();
            });
            view.ipt_interval.onFinishInput += (s) =>
            {
                model.data.model.animTimeInterval = StringHelper.ToFloat(s, 0, true);
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
            DisplayCameraAreaManager.instance.Show();
            Refresh();
        }
        public override void OnHide()
        {
            DisplayCameraAreaManager.instance.Hide();
        }
        public override void Close()
        {

            base.Close();
        }

        public void Refresh()
        {
            styleCon.Clear();
            foreach(ItemStyle style in Enum.GetValues(typeof(ItemStyle)))
            {
                styleCon.Add(new UiStyleParam()
                {
                    style = style
                });
            }
            styleCon.Refresh();

            view.sta_show.ChangeState(model.id == -1?0:1);

            DisplayCameraAreaManager.instance.Clear();
            view.model_axis.SetShow(false);

            float rate = DisplayCameraAreaManager.instance.normalized2scene;
            view.model_axis.SetShow(true, new UiAxisParam()
            {
                pos = new Vector2((model.data.model.subPrefabUnitPos[0].x + rate / 2) / rate, (model.data.model.subPrefabUnitPos[0].z + rate / 2) / rate),
                limitRtf = view.rtf_image,
                onTrsChange = (tp) => {
                    model.data.model.subPrefabUnitPos[0] = new Vector3((tp.Item1.x * 2 - 1) * rate / 2, 0, (tp.Item1.y * 2 - 1) * rate / 2);
                    RefreshView();
                }
            });
            RefreshView();

            if (model.id != -1)
            {
                view.ipt_height.Set(model.data.model.subPrefabUnitScale[0].y.ToString("0.##"));
                view.ipt_length.Set(model.data.model.subPrefabUnitScale[0].z.ToString("0.##"));
                view.ipt_width.Set(model.data.model.subPrefabUnitScale[0].x.ToString("0.##"));
            }
            view.ipt_interval.Set(model.data.model.animTimeInterval.ToString("0.##"));

            itemCon.Clear();
            var texs = model.data.model.subUnitTexsName[0];
            if (texs != null)
            {
                for (int i = 0; i < texs.Count; i++)
                {
                    itemCon.Add(new UiItemParam()
                    {
                        id = i,
                    });
                }
            }
            itemCon.Add(new UiItemParam()
            {
                id = -1,
            });
            itemCon.Refresh();

        }

        private void RefreshView()
        {
            DisplayCameraAreaManager.instance.Clear();
            var showGo = GameManager.instance.utilCtrl.CombineNewObjectByPrefabs("fakeObj", model.data.model, false);
            showGo.SetActive(true);
            DisplayCameraAreaManager.instance.Add(showGo, Vector3.zero);
        }

        public void OnEvent(AssetEvent evt)
        {
            if (active)
                Refresh();
        }
    }
    public partial class UiStyleParam
    {

        public ItemStyle style;
    }
    public partial class UiStyleModel
    {

        public ItemStyle style;
    }
    public partial class UiStyleCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportItemStyleTex(parent.model.data.uid, model.style);
            });
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportItemStyleTex(parent.model.data.uid, model.style);
            });
        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.style = param.style;
            }
            Refresh();
        }

        public void Refresh()
        {
            view.sta_exist.ChangeState(GlobalDefaultHelper.IsInnerAssetName(parent.model.data.styleTex[model.style])?0:1);
            view.txt_.text = TextManager.instance.GetTxt(model.style.ToString());
            view.img_.BindTexData(TexAssetForm.DataById[parent.model.data.styleTex[model.style]]);
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
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateItemModelUnitTex(parent.model.data.uid);
                parent.Refresh();
            });
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
            view.sta_exist.ChangeState(model.id>=0?1:0);
            if (model.id>=0)
            {
                var texs = parent.model.data.model.subUnitTexsName[0];
                var tex = TexAssetForm.DataById.GetDk(texs != null && texs.Count > model.id ? texs[model.id] : GlobalDefaultHelper.DefaultTexId);
                view.txt_.text = tex.name;
                view.img_.BindTexData(tex);

                view.sta_.ChangeState(model.id == parent.model.id ? 1 : 0);
            }
        }
    }
}
