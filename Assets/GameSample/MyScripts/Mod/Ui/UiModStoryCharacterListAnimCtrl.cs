using Form;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Ui.Base;

namespace Ui.ModStoryCharacterListAnim
{
    public partial class UiModStoryCharacterListAnimParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterListAnimModel
    {
        public CharacterProductForm.Data data;
        public string animNm;
        public int part;
        public int id;
    }
    public partial class UiModStoryCharacterListAnimCtrl
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
                ModManager.instance.assetCtrl.DeleteCharacterAnim(model.data.name, model.animNm);

                SetSel();
                Refresh();
            });
            view.btn_deleteId.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacterAnimId(model.data.name, model.animNm, model.part, model.id);

                SetSel(model.animNm, model.part, model.id - 1);
                Refresh();
            });
            view.btn_replace.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportCharacterAnim(model.data.name, model.animNm, model.part, model.id);
                Refresh();
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                ModManager.instance.assetCtrl.RenameCharacterAnim(model.data.name, model.data.animDic[model.animNm].name, s);
                model.animNm = s;
                Refresh();
            };
            view.ipt_interval.onFinishInput += (s) =>
            {
                if (float.TryParse(s, out float v))
                {
                    model.data.animDic[model.animNm].animTimeInterval = v;
                    Refresh();

                }
            };
            view.btn_up.onClick.AddListener(() =>
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
            foreach(var anim in model.data.animDic.Values)
            {
                con.Add(new UiItemParam()
                {
                    nm = anim.name
                });
            }
            con.Add(new UiItemParam()
            {
                nm=""
            });
            con.Refresh();

            animCon.Clear();
            if (!string.IsNullOrEmpty(model.animNm))
            {
                var anim = model.data.animDic[model.animNm];

                for (int i = 0; i < anim.partAnimTexsName[model.part].Count; i++)
                {
                    animCon.Add(new UiUnitParam
                    {
                        id = i
                    });
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
                if (model.id != -1)
                {
                    List<string> texNameLst = new List<string>() {
                anim.partAnimTexsName[0][ model.id],
                 anim.partAnimTexsName[1][ model.id],
                ""
                };
                  

                    var showGo = GameManager.instance.utilCtrl.CombineNewCharacterByPrefabs("fakeChara",texNameLst,false);
                    showGo.SetActive(true);
                    DisplayCameraAreaManager.instance.Add(showGo, Vector3.zero);
                }
            }
            animCon.Refresh();

            view.sta_show.ChangeState(string.IsNullOrEmpty(model.animNm) ? 0 : 1); 
            view.sta_innerId.ChangeState(model.id == -1 ? 0 : 1);
        }
        public void SetSel(string animNm = "", int part = 0, int id = -1)
        {
            model.animNm = animNm;
            model.part = part;
            model.id = id;
        }

    }


    public partial class UiItemParam
    {
        public string nm;
    }
    public partial class UiItemModel
    {
        public string nm;
    }
    public partial class UiItemCtrl
    {
        public override void OnCreate()
        {
            view.btn_item.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(model.nm))
                {
                    ModManager.instance.assetCtrl.CreateCharacterAnim(parent.model.data.name, "");
                    parent.Refresh();
                }
                else
                {
                    parent.SetSel(model.nm, 0);
                    parent.Refresh();
                }
            });
        }
        public override void OnShow()
        {
            model.nm = param.nm;
            Refresh();
        }
        public void Refresh()
        {
            if (!string.IsNullOrEmpty(model.nm))
            {
                view.txt_name.text = parent.model.data.animDic[model.nm].name;
            }
            view.sta_item.ChangeState(!string.IsNullOrEmpty(model.nm) && model.nm == parent.model.animNm ? 1 : 0);
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

                    ModManager.instance.assetCtrl.CreateCharacterAnimId(parent.model.data.name, parent.model.animNm, parent.model.part, parent.model.data.animDic[parent.model.animNm].partAnimTexsName[parent.model.part].Count);
                    parent.Refresh();
                }
                else
                {
                    parent.SetSel(parent.model.animNm, parent.model.part, model.id);
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
                var anim = parent.model.data.animDic[parent.model.animNm];
                view.txt_name.text = anim.name + "_" + model.id;
                view.img_.sprite = TexAssetForm.DataByName[anim.partAnimTexsName[parent.model.part][model.id]].sprite;
            }
            view.sta_unit.ChangeState(model.id != -1 && model.id == parent.model.id ? 1 : 0);
        }

    }
}
