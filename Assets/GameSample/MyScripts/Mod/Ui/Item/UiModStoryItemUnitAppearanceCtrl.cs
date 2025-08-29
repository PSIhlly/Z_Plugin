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
            styleCon = new UiScrViewContainer<UiStyleCtrl>(view.go_style, view.scr_styles);
            itemCon = new UiScrViewContainer<UiItemCtrl> ( view.go_item,view.scr_items);
            view.btn_reset.onClick.AddListener(() =>
            {
                model.data.model.subPrefabUnitScale[model.id] = Vector3.one;
                model.data.model.subPrefabUnitPos[model.id] = Vector3.zero;
                Refresh();
            });
            view.btn_model.onClick.AddListener(() => {
                ModManager.instance.assetCtrl.ChooseModel(TextManager.instance.GetTxt("chooseModel"), (item) =>
                {
                    model.data.model.subPrefabUnitName[model.id] = item.content;
                    Refresh();
                });
            });

            view.btn_delete.onClick.AddListener(() => {
                ModManager.instance.assetCtrl.DeleteItemModelUnit(model.data.name, model.id);
                model.id = -1;
                Refresh();
            });
            view.ipt_height.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[model.id]=model.data.model.subPrefabUnitScale[model.id].NewSetY(StringHelper.ToFloat(s,1,true));
                Refresh();
            };
            view.ipt_length.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[model.id]=model.data.model.subPrefabUnitScale[model.id].NewSetZ(StringHelper.ToFloat(s, 1, true));
                Refresh();
            };
            view.ipt_width.onFinishInput += (s) =>
            {
                model.data.model.subPrefabUnitScale[model.id] = model.data.model.subPrefabUnitScale[model.id].NewSetX(StringHelper.ToFloat(s, 1, true));
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
            view.model_Axis.SetActive(false);

            if (model.id != -1)
            {
                view.ipt_height.Set(model.data.model.subPrefabUnitScale[model.id].y.ToString("0.##"));
                view.ipt_length.Set(model.data.model.subPrefabUnitScale[model.id].z.ToString("0.##"));
                view.ipt_width.Set(model.data.model.subPrefabUnitScale[model.id].x.ToString("0.##"));

                float rate = DisplayCameraAreaManager.instance.normalized2scene;
                view.model_Axis.SetActive(true, new UiAxisParam()
                {
                    pos = new Vector2((model.data.model.subPrefabUnitPos[model.id].x + rate / 2) / rate, (model.data.model.subPrefabUnitPos[model.id].z + rate / 2) / rate),
                    limitRtf = view.rtf_image,
                    onTrsChange = (tp) => {
                        model.data.model.subPrefabUnitPos[model.id] = new Vector3((tp.Item1.x * 2 - 1) * rate / 2, 0, (tp.Item1.y * 2 - 1) * rate / 2);

                        RefreshView();
                    }
                });

                RefreshView();
            }
            itemCon.Clear();
            for(int i=0; i<model.data.model.subPrefabUnitName.Count;i++)
            {
                itemCon.Add(new UiItemParam()
                {
                    id=i,
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
                ModManager.instance.assetCtrl.ImportItemStyleTex(parent.model.data.name, model.style);
            }); 
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportItemStyleTex(parent.model.data.name, model.style);
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
            view.sta_exist.ChangeState(GlobalNameHelper.IsInnerAssetName(parent.model.data.styleTex[model.style])?0:1);
            view.txt_.text = TextManager.instance.GetTxt(model.style.ToString());
            view.img_.sprite = TexAssetForm.DataByName[parent.model.data.styleTex[model.style]].sprite;
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
            view.sta_exist.ChangeState(model.id>=0?1:0);
            if (model.id>=0)
            {
                view.txt_.text = TextManager.instance.GetTxt(parent.model.data.model.subPrefabUnitName[model.id].ToString());
                view.sta_.ChangeState(model.id == parent.model.id ? 1 : 0);
            }
        }
    }
}