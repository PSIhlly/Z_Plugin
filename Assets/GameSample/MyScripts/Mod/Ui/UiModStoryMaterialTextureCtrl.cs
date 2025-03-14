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

namespace Ui.ModStoryMaterial
{
    public partial class UiModStoryMaterialTextureModel
    {
        public MapTextureForm.Data curData;
        public int curAnim;
        private int _playing=-1;
        public Timer animTimer;
        public Action RefreshAct; 
        public int playing
        {
            set
            {
                if (curData == null)
                    return;
                _playing = value;
                if(value!=-1)
                {
                    animTimer = TimeManager.instance.StartTimer(curData.animTimeInterval,()=>
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
        UiScrViewContainer<UiTextureItemCtrl> con;
        UiScrViewContainer<UiAnimTypeItemCtrl> animCon;

        public override void OnCreate()
        {
            this.Register<AssetEvent>();
            model.RefreshAct = Refresh;
            con = new UiScrViewContainer<UiTextureItemCtrl>(view.go_textureItem, view.scr_items);
            animCon = new UiScrViewContainer<UiAnimTypeItemCtrl>(view.go_animTypeItem, view.scr_anims);

            view.btn_replace.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportAnimTex(GlobalHelper.GetTexRealName(model.curData.name, model.curAnim), null);
            });
            view.btn_play.onClick.AddListener(() =>
            {
                if(model.playing>=0)
                    model.playing = -1;
                else
                    model.playing = 0;
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                if (ModManager.instance.assetCtrl.DeleteAnimTex(model.curData.name, model.curAnim))
                {
                    SetCur(null, 0);
                }
                SetCur(model.curData, 0);

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
            SetCur(null, 0);
            Refresh();
        }
        public void Refresh()
        {
            con.Clear();


            foreach (var data in MapTextureForm.DataById.Values)
            {
                if (data.id > MapBaseForm.autoIdCnt)
                    continue;
                con.Add(new UiTextureItemParam
                {
                    id = data.id
                });
            }
            con.Add(new UiTextureItemParam
            {
                id = -1
            });
            con.Refresh();

            view.sta_exist.ChangeState(model.curData == null ? 0 : 1);

            if (model.curData != null)
            {
                animCon.Clear();
                for (int i = 0; i < GlobalSettings.TEX_ANIM_MAX; i++)
                {
                    var curKey = GlobalHelper.GetTexRealName(model.curData.name, i);
                    if (TexAssetForm.DataByName.ContainsKey(curKey))
                    {
                        animCon.Add(new UiAnimTypeItemParam
                        {
                            id = i
                        });
                    }
                    else
                        break;
                }

                animCon.Add(new UiAnimTypeItemParam
                {
                    id = -1
                });


            animCon.Refresh();
            if (model.curData != null)
            {
                view.img_tex.sprite = AssetManager.instance.GetSprite(GlobalHelper.GetTexRealName(model.curData.name, model.playing >= 0? model.playing%(animCon.paramLst.Count-1) : model.curAnim));
                view.ipt_name.Set(model.curData.name);
                view.ipt_intervalSet.Set(model.curData.animTimeInterval.ToString("0.##"));
            }

            view.txt_play.text = TextManager.instance.GetTxt(model.playing>=0?"stop":"play_1");

            }

        }
        public void SetCur(MapTextureForm.Data data,int animId)
        {
                model.curData = data;
            if (animId != -1)
                model.curAnim = animId;
            model.playing = -1;
        }

        public void OnEvent(AssetEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.importAssetName) && isActive)
            {
                string key = GlobalHelper.GetTexNickName(evt.importAssetName);
                if (MapTextureForm.DataByName.ContainsKey(key))
                {
                    SetCur(MapTextureForm.DataByName[key], -1);
                }
            }
            Refresh();
        }

    }

    public partial class UiAnimTypeItemModel
    {
        public int id;
    }
    public partial class UiAnimTypeItemParam
    {
        public int id;
    }
    public partial class UiAnimTypeItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_item.onClick.AddListener(() =>
            {
                if (model.id == -1)
                {
                    string res = null;
                    for (int i = 0; i < GlobalSettings.TEX_ANIM_MAX; i++)
                    {
                        var curKey = GlobalHelper.GetTexRealName(parent.model.curData.name, i);
                        if (!TexAssetForm.DataByName.ContainsKey(curKey))
                        {
                            res = curKey;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(res))
                    {
                        ModManager.instance.assetCtrl.ImportAnimTex(res, null);
                    }
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
                view.img_.sprite = AssetManager.instance.GetSprite(GlobalHelper.GetTexRealName(MapTextureForm.DataById[parent.model.curData.id].name, model.id));
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



    public partial class UiTextureItemModel
    {
        public int id;
    }
    public partial class UiTextureItemParam
    {
        public int id;
    }
    public partial class UiTextureItemCtrl
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

                    var key = GlobalHelper.GetTexRealName("newTex" + max, 0);
                    ModManager.instance.assetCtrl.ImportAnimTex(key, "newTex" + max);
                }
                else
                {
                    parent.SetCur(MapTextureForm.DataById[model.id], 0);
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
                view.img_.sprite = AssetManager.instance.GetSprite(GlobalHelper.GetTexRealName(MapTextureForm.DataById[model.id].name, 0));
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
}

