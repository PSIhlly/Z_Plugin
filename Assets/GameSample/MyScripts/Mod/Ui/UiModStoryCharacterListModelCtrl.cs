using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Ui.Base;

namespace Ui.ModStoryCharacterListModel
{
    public partial class UiModStoryCharacterListModelParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterListModelModel
    {
        public CharacterProductForm.Data data;
        public int animId;
        public int part;
        public int id;
    }
    public partial class UiModStoryCharacterListModelCtrl
    {
        UiScrViewContainer<UiItemCtrl> con;
        UiScrViewContainer<UiUnitCtrl> animCon;
        public override void OnCreate()
        {
            con = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);
            animCon = new UiScrViewContainer<UiUnitCtrl>(view.go_unit, view.scr_units);
            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                var anim = model.data.GetCharacterAnim(model.animId);
                ModManager.instance.assetCtrl.DeleteCharacterAnim(model.data.name, model.animId);
               
                    SetSel();
                Refresh();
            });
            view.btn_deleteId.onClick.AddListener(() =>
            {
                var anim = model.data.GetCharacterAnim(model.animId);
                ModManager.instance.assetCtrl.DeleteCharacterAnimId(model.data.name, model.animId,model.part,model.id);

                SetSel(model.animId, model.part, model.id - 1);
                Refresh();
            });
            view.btn_replace.onClick.AddListener(() =>
            {
                var anim = model.data.GetCharacterAnim(model.animId);
                ModManager.instance.assetCtrl.ImportCharacterAnim(GlobalNameHelper.GetCharacterAnimName(model.data.name, anim.name, model.part, model.id));
                Refresh();
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                var anim = model.data.GetCharacterAnim(model.animId);
                ModManager.instance.assetCtrl.RenameCharacterAnim(model.data.name,anim.name, s);
                Refresh();
            };
            view.ipt_interval.onFinishInput += (s) =>
            {
                var anim = GlobalDataHelper.GetCharacterAnim(model.data, model.animId);
                if(float.TryParse(s,out float v))
                {
                    anim.animTimeInterval = v;
                    model.data.SaveCharacterAnim(model.animId, anim);
                    Refresh();

                }
            };
            view.btn_up.onClick.AddListener(()=>
            {
                model.part = 0;
                Refresh();
            });
            view.btn_down.onClick.AddListener(() =>
            {
                model.part = 1;
                Refresh();
            });

        }
        public override void OnShow()
        {
            DisplayCameraAreaManager.instance.Show();
            model.data = param.data;
            SetSel();
            Refresh();
        }
        public override void OnDisable()
        {
            DisplayCameraAreaManager.instance.Hide();
        }
        public void Refresh()
        {
            con.Clear();
            for(int i=0;i<model.data.animJo.Count;i++)
            {
                con.Add(new UiItemParam()
                {
                    id = i
                });
            }
            con.Add(new UiItemParam()
            {
                id = -1
            });
            con.Refresh();

            animCon.Clear();
            if (model.animId!=-1)
            {
                var anim = model.data.GetCharacterAnim(model.animId);

                for (int i = 0; i < GlobalMaxSettings.CHARACTER_ANIM_MAX; i++)
                {
                    var curKey = GlobalNameHelper.GetCharacterAnimName(model.data.name, anim.name,model.part,i);
                    if (TexAssetForm.DataByName.ContainsKey(curKey))
                    {
                        animCon.Add(new UiUnitParam
                        {
                            id = i
                        });
                    }
                    else
                        break;
                }
                animCon.Add(new UiUnitParam()
                {
                    id = -1
                });

                view.ipt_name.Set(anim.name);
                view.ipt_interval.Set(anim.animTimeInterval.ToString("#0.00"));
                view.sta_up.ChangeState(model.part == 0 ? 1 : 0);
                view.sta_down.ChangeState(model.part == 1 ? 1 : 0);


                DisplayCameraAreaManager.instance.Clear();

                List<string> texNameLst = new List<string>() {
                GlobalNameHelper.GetCharacterAnimName(model.data.name, anim.name,0, model.id),
                GlobalNameHelper.GetCharacterAnimName(model.data.name, anim.name,1, model.id),
                ""
                };
                List<bool> showShaddowLst = new List<bool>()
                {
                    false,false,true
                };

                var showGo = GameManager.instance.utilCtrl.CombineNewGoByPrefabs(
                    "fakeChara", new List<string>() {"Quad","Quad","Capsule"}, texNameLst, new List<Vector3>() { Vector3.up*0.5f, Vector3.up*0.2f,Vector3.zero }, new List<Vector3>() { Vector3.one, Vector3.one,new Vector3(0.3f,0.5f,0.3f) }, showShaddowLst);
                showGo.SetActive(true);

                DisplayCameraAreaManager.instance.Add(showGo, Vector3.zero);

            }
            animCon.Refresh();

            view.sta_show.ChangeState(model.animId == -1 ? 0 : 1);
            view.sta_innerId.ChangeState(model.id == -1 ? 0 : 1);
        }
        public void SetSel(int animId=-1,int part=0,int id=-1)
        {
            model.animId = animId;
            model.part = part;
            model.id = id;
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
            view.btn_item.onClick.AddListener(() =>
            {
                if(model.id == -1)
                {
                    int max = 1;
                    for(int i=0;i<parent.model.data.animJo.Count;i++)
                    {
                        var anim = parent.model.data.GetCharacterAnim(i);
                        var splt = anim.name.Split("newAnim");
                        if (splt.Length > 1)
                        {
                            if (int.TryParse(splt[1], out int v))
                            {
                                max = Mathf.Max(max, v + 1);
                            }
                        }
                    }
                    ModManager.instance.assetCtrl.CreateCharacterAnim(parent.model.data.name, "newAnim"+max);
                    parent.Refresh();
                }
                else
                {
                    parent.SetSel(model.id,0);
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
                view.txt_name.text = parent.model.data.GetCharacterAnim(model.id).name;
            }
            view.sta_item.ChangeState(model.id!=-1&& model.id == parent.model.animId ? 1 : 0);
        }

    }

    public partial class UiUnitParam
    {
        public int id;
    }
    public partial class UiUnitModel
    {
        public int id;
    }
    public partial class UiUnitCtrl
    {
        public override void OnCreate()
        {
            view.btn_unit.onClick.AddListener(() =>
            {
                if (model.id == -1)
                {
                    string res = null;
                    var anim = parent.model.data.GetCharacterAnim(parent.model.animId);
                    for (int i = 0; i < GlobalMaxSettings.CHARACTER_ANIM_MAX; i++)
                    {
                       
                        var curKey = GlobalNameHelper.GetCharacterAnimName(parent.model.data.name, anim.name,parent.model.part, i);
                        if (!TexAssetForm.DataByName.ContainsKey(curKey))
                        {
                            res = curKey;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(res))
                    {
                        ModManager.instance.assetCtrl.ImportCharacterAnim(res);
                    }
                    parent.Refresh();
                }
                else
                {
                    parent.SetSel(parent.model.animId, parent.model.part, model.id);
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
                var anim = parent.model.data.GetCharacterAnim(parent.model.animId);
                view.txt_name.text = anim.name+"_"+model.id;
                view.img_.sprite= TexAssetForm.DataByName[GlobalNameHelper.GetCharacterAnimName(parent.model.data.name, anim.name, parent.model.part,model.id)].sprite;
            }
            view.sta_unit.ChangeState(model.id!=-1&& model .id == parent.model.id ? 1 : 0);
        }

    }
}
