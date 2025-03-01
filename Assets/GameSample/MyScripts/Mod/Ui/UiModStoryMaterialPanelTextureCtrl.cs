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

namespace Ui
{
    public partial class UiModStoryMaterialPanelTextureModel
    {
        public MapTextureForm.Data curData;
        public int curAnim;
    }

    public partial class UiModStoryMaterialPanelTextureCtrl : IZ_Listener<AssetEvent>
    {
        UiScrViewContainer<UiTextureItemCtrl> con;
        UiScrViewContainer<UiAnimTypeItemCtrl> animCon;

        public override void OnCreate()
        {
            this.Register<AssetEvent>();
            con = new UiScrViewContainer<UiTextureItemCtrl>(view.go_textureItem, view.scr_items);
            animCon = new UiScrViewContainer<UiAnimTypeItemCtrl>(view.go_animTypeItem, view.scr_anims);

            view.btn_replace.onClick.AddListener(() =>
            {
                ModAssetManager.instance.ImportAnimTex(ModAssetManager.instance.GetTexRealName(model.curData.name, model.curAnim), null);
            });

            view.ipt_name.onFinishInput += (v) =>
            {
                ModAssetManager.instance.RenameAnimTex(model.curData.name,v);
                Refresh();
            };
            view.ipt_intervalSet.onFinishInput += (v) =>
            {
                if(float.TryParse(v,out var itv))
                {
                    model.curData.animTimeInterval =itv;
                }
                Refresh();
            };
        }


        public override void OnShow()
        {
            model.curData = null;
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
                }) ;
            }
            con.Add(new UiTextureItemParam
            {
                id = -1
            });
            con.Refresh();

            animCon.Clear();

            if (model.curData != null)
            {
                for (int i = 0; i < GlobalSettings.ANIM_MAX; i++)
                {
                    var curKey = ModAssetManager.instance.GetTexRealName( model.curData.name,i);
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
            }
            
            animCon.Refresh();
            if (model.curData != null)
            {
                view.img_tex.sprite = AssetManager.instance.GetSprite(ModAssetManager.instance.GetTexRealName(model.curData.name ,model.curAnim));
                view.ipt_name.Set(model.curData.name);
                view.ipt_intervalSet.Set(model.curData.animTimeInterval.ToString("0.##"));
            }

        }

        public void OnEvent(AssetEvent evt)
        {
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
                    for (int i = 0; i < GlobalSettings.ANIM_MAX; i++)
                    {
                        var curKey = ModAssetManager.instance.GetTexRealName(parent.model.curData.name, i);
                        if (!TexAssetForm.DataByName.ContainsKey(curKey))
                        {
                            res = curKey;
                            break;
                        }
                    }

                    if(!string.IsNullOrEmpty(res))
                    {
                        ModAssetManager.instance.ImportAnimTex(res,null);
                    }
                }
                else
                {
                    parent.model.curAnim = model.id;
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
            if(model.id!=-1)
            {
                view.txt_name.text = parent.model.curData.name + model.id;
                view.img_.sprite = AssetManager.instance.GetSprite(ModAssetManager.instance.GetTexRealName(MapTextureForm.DataById[parent.model.curData.id].name,model.id));
                view.sta_item.ChangeState(parent.model.curAnim == model.id ? 1 : 0);
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
                if(model.id==-1)
                {
                        int max = 1;
                        foreach(var o in MapTextureForm.DataById.Values)
                        {
                            var splt=o.name.Split("newTex");
                            if(splt.Length>1)
                            {
                                if(int.TryParse(splt[1],out int v))
                                {
                                    max = Mathf.Max(max, v+1);
                                }
                            }
                         }

                    var key = ModAssetManager.instance.GetTexRealName("newTex" + max, 0);
                    ModAssetManager.instance.ImportAnimTex(key, "newTex" + max);
                }
                else
                {
                    parent.model.curData = MapTextureForm.DataById[model.id];
                    parent.model.curAnim = 0;
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
            if (model.id!=-1)
            {
                view.img_.sprite = AssetManager.instance.GetSprite(ModAssetManager.instance.GetTexRealName(MapTextureForm.DataById[model.id].name,0));
                view.txt_name.text = MapTextureForm.DataById[model.id].name;
            }
            view.sta_item.ChangeState(parent.model.curData?.id == model.id?1:0);
        }


    }
}

