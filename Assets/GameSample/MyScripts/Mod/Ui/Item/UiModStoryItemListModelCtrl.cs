using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Form;
using Item;
using Z_Texture;
using Z_Ui.Base;
using Z_Ui;
using Z_Map;
using Z_DataSystem.Form;
using Z_DataSystem;
using Z_Language;
using Z_Time;
using Z_Text;
using Z_Math;
using System;

namespace Ui.ModStoryItemListModel
{
    public partial class UiModStoryItemListModelParam
    {
        public ItemProductForm.Data data;

    }
    public partial class UiModStoryItemListModelModel
    {
        public ItemProductForm.Data data;

    }

    public partial class UiModStoryItemListModelCtrl : IZ_Listener<AssetEvent>
    {
        UiScrViewContainer<UiUnitCtrl> unitCon;

        public override void OnCreate()
        {
            this.Register<AssetEvent>();
            unitCon = new UiScrViewContainer<UiUnitCtrl>(view.go_unit, view.scr_units);

            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });

        }

        public override void OnShow()
        {
            if(param!=null)
            {
                model.data = param.data;
            }
            DisplayCameraAreaManager.instance.Show();
            Refresh();
        }

        public override void Close()
        {
            GameManager.instance.saveCtrl.SaveObject(ModManager.instance.GetStoryCoreFolder());

            base.Close();
        }
        public override void OnDisable()
        {
            DisplayCameraAreaManager.instance.Hide();
        }
        public void Refresh()
        {

        


           
                unitCon.Clear();
                for (int i = 0; i < model.data.model.subPrefabUnitName.Count; i++)
                {
                    unitCon.Add(new UiUnitParam
                    {
                        id = i
                    });
                }
                unitCon.Add(new UiUnitParam
                {
                    id = -1
                });
                unitCon.Refresh();


                DisplayCameraAreaManager.instance.Clear();

                var showGo= GameManager.instance.utilCtrl.CombineNewObjectByPrefabs(
                model.data.name, model.data.model, false);
                showGo.SetActive(true);
                DisplayCameraAreaManager.instance.Add(showGo, Vector3.zero);
        }


        public void OnEvent(AssetEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.importAssetName) && isActive)
            {
                /*string key = GlobalNameHelper.GetTexNickName(evt.importAssetName);
                if (MapObjectForm.DataByName.ContainsKey(key))
                {
                    SetCur(MapObjectForm.DataByName[key]);
                }*/
            }
            Refresh();
        }

    }

    public partial class UiUnitModel
    {
        public int id;
    }
    public partial class UiUnitParam
    {
        public int id;
    }
    public partial class UiUnitCtrl
    {
        public override void OnCreate()
        {
            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateItemModelUnit(parent.model.data);
                parent.Refresh();
            });
            view.btn_prefab.onClick.AddListener(() =>
            {
                var pre=MapPrefabForm.DataByName[parent.model.data.model.subPrefabUnitName[model.id]];
                parent.model.data.model.subPrefabUnitName[model.id] = MapPrefabForm.DataById[((pre.id) % MapPrefabForm.DataById.Count) + 1].name;
                parent.Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteObjectUnit(parent.model.data.name,model.id);
                parent.Refresh();
            });
            view.ipt_posX.onFinishInput += s =>
              {
                  if (float.TryParse(s, out var v))
                  {
                      parent.model.data.model.subPrefabUnitPos[model.id] =
                      parent.model.data.model.subPrefabUnitPos[model.id].SetX(v);
                      parent.Refresh();
                  }
              };
            view.ipt_posY.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.data.model.subPrefabUnitPos[model.id] =
                    parent.model.data.model.subPrefabUnitPos[model.id].SetY(v);
                    parent.Refresh();
                }
            };
            view.ipt_posZ.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.data.model.subPrefabUnitPos[model.id] =
                    parent.model.data.model.subPrefabUnitPos[model.id].SetZ(v);
                    parent.Refresh();
                }
            };

            view.ipt_scaleX.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.data.model.subPrefabUnitScale[model.id] =
                    parent.model.data.model.subPrefabUnitScale[model.id].SetX(v);
                    parent.Refresh();
                }
            };
            view.ipt_scaleY.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.data.model.subPrefabUnitScale[model.id] =
                    parent.model.data.model.subPrefabUnitScale[model.id].SetY(v);
                    parent.Refresh();
                }
            };
            view.ipt_scaleZ.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.data.model.subPrefabUnitScale[model.id] =
                    parent.model.data.model.subPrefabUnitScale[model.id].SetZ(v);
                    parent.Refresh();
                }
            };

            view.btn_tex.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportItemModelUnitTex(parent.model.data.name,model.id);
            });
        }
        public override void OnShow()
        {
            model.id = param.id;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_unit.ChangeState(model.id != -1 ? 1 : 0);
            if (model.id != -1)
            {
                var modelCache = parent.model.data.model;
                view.img_tex.sprite = TexAssetForm.DataByName[modelCache.subUnitTexsName[model.id]].sprite;

                view.ipt_posX.Set(modelCache.subPrefabUnitPos[model.id].x.ToString("0.##"));
                view.ipt_posY.Set(modelCache.subPrefabUnitPos[model.id].y.ToString("0.##"));
                view.ipt_posZ.Set(modelCache.subPrefabUnitPos[model.id].z.ToString("0.##"));

                view.ipt_scaleX.Set(modelCache.subPrefabUnitScale[model.id].x.ToString("0.##"));
                view.ipt_scaleY.Set(modelCache.subPrefabUnitScale[model.id].y.ToString("0.##"));
                view.ipt_scaleZ.Set(modelCache.subPrefabUnitScale[model.id].z.ToString("0.##"));

            }
            else
            {
                view.txt_name.text = TextManager.instance.GetTxt("new");
                /*view.img_.sprite = Texture2D.whiteTexture;*/
            }
        }


    }

}

