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

namespace Ui.ModStoryObject
{
    public partial class UiModStoryObjectModel
    {
        public MapObjectForm.Data curData;

    }

    public partial class UiModStoryObjectCtrl : IZ_Listener<AssetEvent>
    {
        UiScrViewContainer<UiItemCtrl> con;
        UiScrViewContainer<UiUnitCtrl> unitCon;

        public override void OnCreate()
        {
            this.Register<AssetEvent>();
            unitCon = new UiScrViewContainer<UiUnitCtrl>(view.go_unit, view.scr_units);
            con = new UiScrViewContainer<UiItemCtrl>(view.go_Item, view.scr_items);


            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteObject(model.curData.name);
                SetCur(null);

                Refresh();
            });
            view.ipt_name.onFinishInput += (v) =>
            {
                ModManager.instance.assetCtrl.RenameObject(model.curData.name, v);
                Refresh();
            };
            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });

        }

        public override void OnShow()
        {
            DisplayCameraAreaManager.instance.Show();
            SetCur(null);
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
            con.Clear();


            foreach (var data in MapObjectForm.DataById.Values)
            {
                if (data.id > MapBaseForm.autoIdCnt)
                    continue;
                con.Add(new UiItemParam
                {
                    id = data.id
                });
            }
            con.Add(new UiItemParam
            {
                id = -1
            });
            con.Refresh();


            if (model.curData != null)
            {
                unitCon.Clear();
                for (int i = 0; i < model.curData.model.subPrefabUnitName.Count; i++)
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
                if (model.curData != null)
                {
                    view.ipt_name.Set(model.curData.name);
                }

                DisplayCameraAreaManager.instance.Clear();

                var showGo= GameManager.instance.utilCtrl.CombineNewObjectByPrefabs(
                    model.curData.name, model.curData.model, false);
                showGo.SetActive(true);
                DisplayCameraAreaManager.instance.Add(showGo, Vector3.zero);

            }

            view.sta_exist.ChangeState(model.curData == null ? 0 : 1);
        }

        public void SetCur(MapObjectForm.Data data)
        {
            model.curData = data;
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
                ModManager.instance.assetCtrl.CreateObjectUnit(parent.model.curData);
                parent.Refresh();
            });
            view.btn_prefab.onClick.AddListener(() =>
            {
                var pre=MapPrefabForm.DataByName[parent.model.curData.model.subPrefabUnitName[model.id]];
                parent.model.curData.model.subPrefabUnitName[model.id] = MapPrefabForm.DataById[((pre.id) % MapPrefabForm.DataById.Count) + 1].name;
                parent.Refresh();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteObjectUnit(parent.model.curData.name,model.id);
                parent.Refresh();
            });
            view.ipt_posX.onFinishInput += s =>
              {
                  if (float.TryParse(s, out var v))
                  {
                      parent.model.curData.model.subPrefabUnitPos[model.id] =
                      parent.model.curData.model.subPrefabUnitPos[model.id].SetX(v);
                      parent.Refresh();
                  }
              };
            view.ipt_posY.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.curData.model.subPrefabUnitPos[model.id] =
                    parent.model.curData.model.subPrefabUnitPos[model.id].SetY(v);
                    parent.Refresh();
                }
            };
            view.ipt_posZ.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.curData.model.subPrefabUnitPos[model.id] =
                    parent.model.curData.model.subPrefabUnitPos[model.id].SetZ(v);
                    parent.Refresh();
                }
            };

            view.ipt_scaleX.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.curData.model.subPrefabUnitScale[model.id] =
                    parent.model.curData.model.subPrefabUnitScale[model.id].SetX(v);
                    parent.Refresh();
                }
            };
            view.ipt_scaleY.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.curData.model.subPrefabUnitScale[model.id] =
                    parent.model.curData.model.subPrefabUnitScale[model.id].SetY(v);
                    parent.Refresh();
                }
            };
            view.ipt_scaleZ.onFinishInput += s =>
            {
                if (float.TryParse(s, out var v))
                {
                    parent.model.curData.model.subPrefabUnitScale[model.id] =
                    parent.model.curData.model.subPrefabUnitScale[model.id].SetZ(v);
                    parent.Refresh();
                }
            };

            view.btn_tex.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportObjectTex(parent.model.curData.name,model.id);
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
                var modelCache = parent.model.curData.model;
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



    public partial class UiItemModel
    {
        public int id;
    }
    public partial class UiItemParam
    {
        public int id;
    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_item.onClick.AddListener(() =>
            {
                if (model.id == -1)
                {
                    
                    ModManager.instance.assetCtrl.CreateObject("");
                    parent.Refresh();
                }
                else
                {
                    parent.SetCur(MapObjectForm.DataById[model.id]);
                    parent.Refresh();
                }
            });

            
        }
        public override void OnShow()
        {
            model.id = param.id;
            Refresh();
        }
        public void Refresh()
        {
            if (model.id != -1)
            {
                view.img_.sprite = TexAssetForm.DataByName[MapObjectForm.DataById[model.id].model.subUnitTexsName.Count>0? MapObjectForm.DataById[model.id].model.subUnitTexsName [0]: ""].sprite;
                view.txt_name.text = MapObjectForm.DataById[model.id].name;
            }
            else
            {
                view.txt_name.text = TextManager.instance.GetTxt("new");
                //view.img_.sprite = AssetManager.instance.GetSprite(ModAssetManager.instance.GetTexRealName(MapTextureForm.DataById[model.id].name, 0));
            }
            view.sta_item.ChangeState(parent.model.curData?.id == model.id ? 1 : 0);
        }


    }
}

