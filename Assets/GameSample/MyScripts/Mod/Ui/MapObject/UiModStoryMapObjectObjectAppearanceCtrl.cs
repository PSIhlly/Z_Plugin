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
using Z_DataSystem;
using Ui.Axis;
using Z_DesignStyle;

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
    public partial class UiModStoryMapObjectObjectAppearanceCtrl:IZ_Listener<AssetEvent>
    {
        UiScrViewContainer<UiItemCtrl> itemCon;
        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            itemCon = new UiScrViewContainer<UiItemCtrl>(this, view.go_item, view.scr_items);
            view.btn_reset.onClick.AddListener(() =>
            {
                model.data.model.subPrefabUnitScale[0] = Vector3.one;
                model.data.model.subPrefabUnitPos[0] = Vector3.zero;
                Refresh();
            });
            view.btn_model.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseModel(TextManager.instance.GetTxt("chooseModel"), (item) =>
                {
                    model.data.model.subPrefabUnitName[0] = item.id;
                    Refresh();
                });

            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteObject(model.data.name);
                parent.parent.SelType(3);
                Refresh();
            });
            view.btn_deleteUnit.onClick.AddListener(() =>
            {
                if (model.id >= 0)
                {
                    ModManager.instance.assetCtrl.DeleteObjectUnitTex(model.data.name, model.id);
                    model.id = -1;
                    Refresh();
                }
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                model.data.name = s;
                Refresh();
            };
            view.ipt_label.onFinishInput += (s) =>
            {
                model.data.label = s;
                Refresh();
            };
            view.ipt_posHeight.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitPos[0] = model.data.model.subPrefabUnitPos[0].NewSetY(StringHelper.ToFloat(s, 0, false));
                Refresh();
            };
            view.ipt_height.onFinishInput += (s) =>
            {

                model.data.model.subPrefabUnitScale[0]=model.data.model.subPrefabUnitScale[0].NewSetY(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.ipt_length.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[0]= model.data.model.subPrefabUnitScale[0].NewSetZ(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.ipt_width.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[0]= model.data.model.subPrefabUnitScale[0].NewSetX(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.btn_image.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportObjectUnitTex(model.data.id, model.id);
                Refresh();
            });
            view.ipt_interval.onFinishInput += (s) =>
            {
                model.data.model.animTimeInterval = StringHelper.ToFloat(s, 0, true);
                Refresh();
            };
            view.ipt_colliderScale.onFinishInput += (s) =>
            {
                model.data.model.colliderScale = StringHelper.ToFloat(s, 1, true);
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

        public void Refresh()
        {
            view.sta_show.ChangeState(model.id == -1 ? 0 : 1);

            view.ipt_name.Set(model.data.name);
            view.ipt_label.Set(model.data.label);

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
                view.ipt_posHeight.Set(model.data.model.subPrefabUnitPos[0].y.ToString("0.##"));
                view.ipt_height.Set(model.data.model.subPrefabUnitScale[0].y.ToString("0.##"));
                view.ipt_length.Set(model.data.model.subPrefabUnitScale[0].z.ToString("0.##"));
                view.ipt_width.Set(model.data.model.subPrefabUnitScale[0].x.ToString("0.##"));
            }
            view.ipt_interval.Set(model.data.model.animTimeInterval.ToString("0.##"));
            view.ipt_colliderScale.Set(model.data.model.colliderScale.ToString("0.##"));

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
                ModManager.instance.assetCtrl.CreateObjectUnitTex(parent.model.data.id);
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
            view.sta_exist.ChangeState(model.id >= 0 ? 1 : 0);
            if (model.id >= 0)
            {
                var texs = parent.model.data.model.subUnitTexsName[0];
                var tex = TexAssetForm.DataById.GetDv(texs != null && texs.Count > model.id ? texs[model.id] : -1,null);
                view.txt_.text = "";
                if (tex != null)
                {
                    view.img_.BindTexData(tex);
                }
                view.sta_.ChangeState(model.id == parent.model.id ? 1 : 0);
            }
        }
    }
}
