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
            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);
            view.btn_reset.onClick.AddListener(() =>
            {
                model.data.model.subPrefabUnitScale[model.id] = Vector3.one;
                model.data.model.subPrefabUnitPos[model.id] = Vector3.zero;
                Refresh();
            });
            view.btn_model.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseModel(TextManager.instance.GetTxt("chooseModel"), (item) =>
                {
                    model.data.model.subPrefabUnitName[model.id] = item.content;
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

                model.data.model.subPrefabUnitScale[model.id]=model.data.model.subPrefabUnitScale[model.id].NewSetY(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.ipt_length.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[model.id]= model.data.model.subPrefabUnitScale[model.id].NewSetZ(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.ipt_width.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[model.id]= model.data.model.subPrefabUnitScale[model.id].NewSetX(StringHelper.ToFloat(s, 1, true));
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
            GameManager.instance.saveCtrl.SaveItem(ModManager.instance.GetStoryCoreFolder());
        }
        public void Refresh()
        {
            view.sta_show.ChangeState(model.id == -1 ? 0 : 1);

            view.ipt_name.Set(model.data.name);

            DisplayCameraAreaManager.instance.Clear();

            view.model_Axis.SetActive(false);
            if (model.id != -1)
            {
                view.ipt_height.Set(model.data.model.subPrefabUnitScale[model.id].y.ToString("0.##"));
                view.ipt_length.Set(model.data.model.subPrefabUnitScale[model.id].z.ToString("0.##"));
                view.ipt_width.Set(model.data.model.subPrefabUnitScale[model.id].x.ToString("0.##"));

                float rate = DisplayCameraAreaManager.instance.normalized2scene;
                view.model_Axis.SetActive(true, new UiAxisParam()
                {
                    pos = new Vector2((model.data.model.subPrefabUnitPos[model.id].x+ rate/2)/rate,( model.data.model.subPrefabUnitPos[model.id].z+rate/2)/rate),
                    limitRtf = view.rtf_image,
                    onTrsChange = (tp) => {
                        model.data.model.subPrefabUnitPos[model.id] = new Vector3((tp.Item1.x*2-1) * rate / 2, 0, (tp.Item1.y*2-1) * rate / 2);
                        
                        RefreshView();
                    }
                });
                RefreshView();
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
                view.txt_.text = TextManager.instance.GetTxt(parent.model.data.model.subPrefabUnitName[model.id].ToString());
                view.sta_.ChangeState(model.id == parent.model.id ? 1 : 0);
            }
        }
    }
}