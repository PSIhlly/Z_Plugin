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
using System;

namespace Ui.ModStoryMaterial.ModStoryMaterialMask
{
    public partial class UiModStoryMaterialMaskModel
    {
        public MapMaskForm.Data curData;
        public int curMask;
        
    }

    public partial class UiModStoryMaterialMaskCtrl : IZ_Listener<AssetEvent>
    {
        UiScrViewContainer<UiItemCtrl> con;
        UiScrViewContainer<UiUnitCtrl> maskCon;

        public override void OnCreate()
        {
            this.Register<AssetEvent>();
            con = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);
            maskCon = new UiScrViewContainer<UiUnitCtrl>(view.go_unit, view.scr_masks);

            view.btn_replace.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportMaskTex(model.curData.name, model.curMask);
            });

            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteMaskTex(model.curData.name);
                SetCur(null, 0);

                Refresh();
            });
            view.ipt_name.onFinishInput += (v) =>
            {
                ModManager.instance.assetCtrl.RenameMaskTex(model.curData.name, v);
                Refresh();
            };
            
        }

        public override void OnShow()
        {
            SetCur(null, 0);
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();


            foreach (var data in MapMaskForm.DataById.Values)
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

            view.sta_exist.ChangeState(model.curData == null ? 0 : 1);

            if (model.curData != null)
            {
                maskCon.Clear();
                for (int i = 0; i < model.curData.texsName.Count; i++)
                {
                    maskCon.Add(new UiUnitParam
                    {
                        id = i
                    });
                }

            maskCon.Refresh();
            if (model.curData != null)
            {
                view.img_tex.sprite = TexAssetForm.DataByName[model.curData.texsName[model.curMask]].sprite;
                view.ipt_name.Set(model.curData.name);
            }
            switch((AlphaTexBasic5)model.curMask)
            {
                    case AlphaTexBasic5.OOOOXOOOO:
                        SetShow(new HashSet<int>() { });
                        break;
                    case AlphaTexBasic5.OOOXXOOOO:
                        SetShow(new HashSet<int>() {4 });
                        break;
                    case AlphaTexBasic5.OXOXXOOOO:
                        SetShow(new HashSet<int>() {1,2,3,4,6,8,9 });
                        break;
                    case AlphaTexBasic5.OOXOOXXXX:
                        SetShow(new HashSet<int>() {4,7,8 });
                        break;
                    case AlphaTexBasic5.XXXOOOOOO:
                        SetShow(new HashSet<int>() {4,6 });
                        break;
                    case AlphaTexBasic5.XXXXOXXXX:
                        SetShow(new HashSet<int>() {1,2,3,4,6,7,8,9 });
                        break;
                }

            }

        }
        private void SetShow(HashSet<int> show)
        {
            view.img_1.color = show.Contains(1) ? Color.green : Color.white;
            view.img_2.color = show.Contains(2) ? Color.green : Color.white;
            view.img_3.color = show.Contains(3) ? Color.green : Color.white;
            view.img_4.color = show.Contains(4) ? Color.green : Color.white;
            view.img_5.color = Color.yellow;
            view.img_6.color = show.Contains(6) ? Color.green : Color.white;
            view.img_7.color = show.Contains(7) ? Color.green : Color.white;
            view.img_8.color = show.Contains(8) ? Color.green : Color.white;
            view.img_9.color = show.Contains(9) ? Color.green : Color.white;
        }
        public void SetCur(MapMaskForm.Data data,int maskId)
        {
                model.curData = data;
            if (maskId != -1)
                model.curMask = maskId;
        }

        public void OnEvent(AssetEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.importAssetName)&&isActive)
            {
                /*string key = GlobalNameHelper.GetTexNickName(evt.importAssetName);
                if (MapMaskForm.DataByName.ContainsKey(key))
                {
                    SetCur(MapMaskForm.DataByName[key], -1);
                }*/
            }
            Refresh();
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
                    
                    ModManager.instance.assetCtrl.CreateMaskTex("");
                    parent.Refresh();
                }
                else
                {
                    parent.SetCur(MapMaskForm.DataById[model.id], 0);
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
                view.img_.sprite = TexAssetForm.DataByName[MapMaskForm.DataById[model.id].texsName.Count>0? MapMaskForm.DataById[model.id].texsName[0]: ""].sprite;
                view.txt_name.text = MapMaskForm.DataById[model.id].name;
            }
            else
            {
                view.txt_name.text = TextManager.instance.GetTxt("new");
                //view.img_.sprite = AssetManager.instance.GetSprite(ModAssetManager.instance.GetTexRealName(MapTextureForm.DataById[model.id].name, 0));
            }
            view.sta_item.ChangeState(parent.model.curData?.id == model.id ? 1 : 0);
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
            view.btn_item.onClick.AddListener(() =>
            {
                
                    parent.SetCur(parent.model.curData, model.id);
                    parent.Refresh();
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
                view.txt_name.text = parent.model.curData.name + "_" + model.id;
                view.img_.sprite = TexAssetForm.DataByName[MapMaskForm.DataById[parent.model.curData.id].texsName[ model.id]].sprite;
                view.sta_item.ChangeState(parent.model.curMask == model.id ? 1 : 0);
            }
            else
            {
                view.txt_name.text = TextManager.instance.GetTxt("new");
                /*view.img_.sprite = Texture2D.whiteTexture;*/
                view.sta_item.ChangeState(0);
            }
        }


    }



    
}

