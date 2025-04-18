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

namespace Ui.ModStoryMaterial.ModStoryMaterialTexture
{
    public partial class UiModStoryMaterialTextureModel
    {
        public MapTextureForm.Data curData;
        public int curAnim;
        private int _playing = -1;
        public Timer animTimer;
        public Action RefreshAct;
        public int playing
        {
            set
            {
                if (curData == null)
                    return;
                _playing = value;
                if (value != -1)
                {
                    animTimer = TimeManager.instance.StartTimer(0,curData.animTimeInterval, () =>
                     {
                         _playing++;
                         RefreshAct?.Invoke();

                         return false;
                     });
                }
                else
                {
                    TimeManager.instance.CancelTimer(animTimer);
                }
                RefreshAct?.Invoke();
            }
            get
            {
                return _playing;
            }
        }
    }

    public partial class UiModStoryMaterialTextureCtrl : IZ_Listener<AssetEvent>
    {
        UiScrViewContainer<UiItemCtrl> con;
        UiScrViewContainer<UiUnitCtrl> animCon;

        public override void OnCreate()
        {
            this.Register<AssetEvent>();
            model.RefreshAct = Refresh;
            con = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);
            animCon = new UiScrViewContainer<UiUnitCtrl>(view.go_unit, view.scr_anims);

            view.btn_replace.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportAnimTex(model.curData.name, model.curAnim);
            });
            view.btn_play.onClick.AddListener(() =>
            {
                if (model.playing >= 0)
                    model.playing = -1;
                else
                    model.playing = 0;
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteAnimTex(model.curData.name);
                SetCur();
                Refresh();
            });
            view.btn_deleteId.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteAnimTexId(model.curData.name, model.curAnim);
                SetCur(model.curData, model.curAnim - 1);

                Refresh();
            });


            view.ipt_name.onFinishInput += (v) =>
                {
                    ModManager.instance.assetCtrl.RenameAnimTex(model.curData.name, v);
                    Refresh();
                };
            view.ipt_intervalSet.onFinishInput += (v) =>
            {
                if (float.TryParse(v, out var itv))
                {
                    model.curData.animTimeInterval = itv;
                }
                model.playing = -1;
                Refresh();
            };
        }
        public override void OnDisable()
        {
            model.playing = -1;
        }

        public override void OnShow()
        {
            SetCur();
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();


            foreach (var data in MapTextureForm.DataById.Values)
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

            animCon.Clear();
            if (model.curData != null)
            {

                for (int i = 0; i < model.curData.texsName.Count; i++)
                {
                    animCon.Add(new UiUnitParam
                    {
                        id = i
                    });
                }

                animCon.Add(new UiUnitParam
                {
                    id = -1
                });



                if (model.curData != null)
                {
                    if (model.curAnim != -1)
                        view.img_tex.sprite = TexAssetForm.DataByName[model.curData.texsName[model.playing >= 0 ? model.playing % (animCon.paramLst.Count - 1) : model.curAnim]].sprite;
                    else
                        view.img_tex.sprite = TextureHelper.transparentSprite;
                    view.ipt_name.Set(model.curData.name);
                    view.ipt_intervalSet.Set(model.curData.animTimeInterval.ToString("0.##"));
                }

                view.txt_play.text = TextManager.instance.GetTxt(model.playing >= 0 ? "stop" : "play_1");

            }
            animCon.Refresh();

            view.sta_show.ChangeState(model.curData == null ? 0 : 1);
            view.sta_innerId.ChangeState(model.curAnim == -1 ? 0 : 1);
        }
        public void SetCur(MapTextureForm.Data data = null, int animId = -1)
        {
            model.curData = data;
            model.curAnim = animId;
            model.playing = -1;
        }

        public void OnEvent(AssetEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.importAssetName) && isActive)
            {
                /*                string key = GlobalNameHelper.GetTexNickName(evt.importAssetName);
                                if (MapTextureForm.DataByName.ContainsKey(key))
                                {
                                    SetCur(MapTextureForm.DataByName[key], -1);
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
                    int max = 1;
                    foreach (var o in MapTextureForm.DataById.Values)
                    {
                        var splt = o.name.Split("newTex");
                        if (splt.Length > 1)
                        {
                            if (int.TryParse(splt[1], out int v))
                            {
                                max = Mathf.Max(max, v + 1);
                            }
                        }
                    }

                    ModManager.instance.assetCtrl.CreateAnimTex("newTex" + max);
                    parent.Refresh();
                }
                else
                {
                    parent.SetCur(MapTextureForm.DataById[model.id], -1);
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
                view.img_.sprite = TexAssetForm.DataByName[MapTextureForm.DataById[model.id].texsName.Count > 0 ? MapTextureForm.DataById[model.id].texsName[0] : ""].sprite;
                view.txt_name.text = MapTextureForm.DataById[model.id].name;
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
                if (model.id == -1)
                {

                    parent.model.curData.texsName.Add("");
                    parent.Refresh();
                }
                else
                {
                    parent.SetCur(parent.model.curData, model.id);
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
                view.txt_name.text = parent.model.curData.name + "_" + model.id;
                view.img_.sprite = TexAssetForm.DataByName[parent.model.curData.texsName[model.id]].sprite;
                view.sta_item.ChangeState(parent.model.curAnim == model.id ? 1 : 0);
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

