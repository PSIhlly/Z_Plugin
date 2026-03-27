using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
using Unity.VisualScripting;
using Z_Text;
using Z_Ui.Notify;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DataSystem;
using Ui.Axis;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitAppearance.ModStoryCharacterUnitAppearanceUnit
{

    public partial class UiModStoryCharacterUnitAppearanceUnitParam
    {

        public CharacterAnimForm.Data data;
    }
    public partial class UiModStoryCharacterUnitAppearanceUnitModel
    {

        public CharacterAnimForm.Data data;
        public BodyPartType part;
        public EquipPartType equipPart;
        public int id;
    }
    public partial class UiModStoryCharacterUnitAppearanceUnitCtrl : IZ_Listener<AssetEvent>
    {

        UiScrViewContainer<UiItemCtrl> itemCon;
        UiScrViewContainer<UiPartCtrl> partCon;
        UiScrViewContainer<UiEquipPartCtrl> equipPartCon;
        UiScrViewContainer<UiToggleCtrl> enablePartCon;
        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacterAnim(parent.model.data.uid, model.data.name);
                parent.SelPage(0);
            });
            view.ipt_name.onFinishInput += (s) =>
            {
                if (StringHelper.IsUniqueName(parent.model.data.animDic.Keys, s))
                    ModManager.instance.assetCtrl.RenameCharacterAnim(parent.model.data.uid, model.data.name, s);
                Refresh();
            };
            view.ipt_scale.onFinishInput += (s) =>
            {
                model.data.scale = StringHelper.ToFloat(s, 1, true);
                Refresh();
            };
            view.ipt_interval.onFinishInput += (s) =>
            {
                model.data.animTimeInterval = StringHelper.ToFloat(s, 0.2f, true);
                Refresh();
            };
            view.ipt_priority.onFinishInput += (s) =>
            {
                model.data.priority = StringHelper.ToInt(s, 1, true);
                Refresh();
            };

            view.btn_deleteTex.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacterAnimId(parent.model.data.uid, model.data.name, model.id);
                model.id = -1;
                model.part = BodyPartType.None;
                Refresh();
            });
            view.btn_resetTex.onClick.AddListener(() =>
            {
                model.data.animClip[model.id].partTex[model.part] = GlobalNameHelper.GetDefaultTexName();
                Refresh();
            });
            view.btn_image.onClick.AddListener(() =>
            {
                if (model.equipPart == EquipPartType.None)
                {
                    ModManager.instance.assetCtrl.ImportCharacterAnim(parent.model.data.uid, model.data.name, model.part, model.id);
                }
            });
            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);

            enablePartCon = new UiScrViewContainer<UiToggleCtrl>(view.go_toggle, view.scr_enableParts);
            partCon = new UiScrViewContainer<UiPartCtrl>(view.go_part, view.scr_parts);
            equipPartCon = new UiScrViewContainer<UiEquipPartCtrl>(view.go_equipPart, view.scr_equipParts);
            view.btn_plus.onClick.AddListener(() =>
            {
                var o = model.data.animClip[model.id].equipTrs[model.equipPart];
                model.data.animClip[model.id].equipTrs[model.equipPart] = (o.Item1 + 1, o.Item2, o.Item3, o.Item4);
                Refresh();
            });
            view.btn_minus.onClick.AddListener(() =>
            {
                var o = model.data.animClip[model.id].equipTrs[model.equipPart];
                model.data.animClip[model.id].equipTrs[model.equipPart] = (o.Item1 - 1, o.Item2, o.Item3, o.Item4);
                Refresh();
            });
            view.btn_itemStyle.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseItemStyle(TextManager.instance.GetTxt("chooseModel"), (item) =>
                {
                    model.data.animClip[model.id].equipStyle[model.equipPart] = (ItemStyle)Enum.Parse(typeof(ItemStyle), item.content);
                    Refresh();
                });
            });

        }
        public override void OnShow()
        {
            model.data = param.data;
            DisplayCameraAreaManager.instance.Show();
            model.id = -1;
            model.part = BodyPartType.UpperPart;
            model.equipPart = EquipPartType.None;
            Refresh();
        }
        public override void OnHide()
        {
            DisplayCameraAreaManager.instance.Hide();
        }
        public void Refresh()
        {
            view.ipt_name.Set(model.data.name);
            view.ipt_scale.Set(model.data.scale.ToString());
            view.ipt_interval.Set(model.data.animTimeInterval.ToString());
            view.ipt_priority.Set(model.data.priority.ToString());

            view.sta_show.ChangeState(model.id == -1 ? 0 : 1);
            view.sta_equip.ChangeState(model.equipPart == EquipPartType.None ? 0 : 1);

            enablePartCon.Clear();
            foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
            {
                if (part != BodyPartType.None)
                    enablePartCon.Add(new UiToggleParam()
                    {
                        tp = part
                    });
            }
            enablePartCon.Refresh();

            equipPartCon.Clear();
            partCon.Clear();

            DisplayCameraAreaManager.instance.Clear();

            view.sta_equip.ChangeState(model.id > -1 && model.equipPart != EquipPartType.None ? 1 : 0);
            view.model_axis.SetShow(false);
            if (model.id > -1)
            {

                view.sta_equip.ChangeState(0);

                foreach (EquipPartType equipPart in Enum.GetValues(typeof(EquipPartType)))
                {
                    if (equipPart != EquipPartType.None)
                        equipPartCon.Add(new UiEquipPartParam()
                        {
                            tp = equipPart
                        });
                }

                foreach (BodyPartType part in model.data.partEnable.Keys)
                {
                    if (part != BodyPartType.None && model.data.partEnable[part])
                        partCon.Add(new UiPartParam()
                        {
                            tp = part
                        });
                }

                if (model.equipPart != EquipPartType.None)
                {
                    view.txt_layer.text = model.data.animClip[model.id].equipTrs[model.equipPart].Item3.ToString();

                    /*                  view.model_Axis.SetActive(true, new UiAxisParam()
                                      {
                                          pos = new Vector2((model.data.model.subPrefabUnitPos[model.id].x + rate / 2) / rate, (model.data.model.subPrefabUnitPos[model.id].z + rate / 2) / rate),
                                          limitRtf = view.rtf_image,
                                          onTrsChange = (tp) => {
                                              model.data.model.subPrefabUnitPos[model.id] = new Vector3((tp.Item1.x * 2 - 1) * rate / 2, 0, (tp.Item1.y * 2 - 1) * rate / 2);

                                              RefreshView();
                                          }
                                      });*/
                }


                RefreshView();
            }

            equipPartCon.Refresh();
            partCon.Refresh();


            itemCon.Clear();
            for (int i = 0, icnt = model.data.animClip.Count; i < icnt; i++)
            {
                itemCon.Add(new UiItemParam()
                {
                    id = i
                });
            }
            itemCon.Add(new UiItemParam()
            {
                id = -1
            });
            itemCon.Refresh();


        }
        private void RefreshView()
        {
            List<string> texNameLst = new List<string>() {
                model.data.animClip[model.id].partTex[BodyPartType.UpperPart],
                null,
                GlobalNameHelper.GetDefaultTexName()
                };
            var showGo = GameManager.instance.utilCtrl.CombineNewCharacterByPrefabs("fakeChara", texNameLst, false);
            showGo.SetActive(true);
            DisplayCameraAreaManager.instance.Add(showGo, Vector3.zero);
        }

        public void OnEvent(AssetEvent evt)
        {
            if (active)
                Refresh();
        }
    }
    public partial class UiToggleParam
    {
        public BodyPartType tp;
    }
    public partial class UiToggleModel
    {
        public BodyPartType tp;

    }
    public partial class UiToggleCtrl
    {

        public override void OnCreate()
        {

            view.btn_enablePart.onClick.AddListener(() =>
            {
                if (!parent.model.data.partEnable.ContainsKey(model.tp))
                {
                    parent.model.data.partEnable[model.tp] = false;
                }
                parent.model.data.partEnable[model.tp] = !parent.model.data.partEnable[model.tp];
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.tp = param.tp;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = TextManager.instance.GetTxt(model.tp.ToString());
            view.sta_enablePart.ChangeState(parent.model.data.partEnable.ContainsKey(model.tp) && parent.model.data.partEnable[model.tp] ? 1 : 0);

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

            view.btn_new.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateCharacterAnimId(parent.parent.model.data.uid, parent.model.data.name);
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.id = model.id;
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
            view.sta_exist.ChangeState(model.id == -1 ? 0 : 1);
            if (model.id != -1)
            {
                view.txt_.text = "";
                view.img_.sprite = TexAssetForm.DataByName[parent.model.data.animClip[model.id].partTex[BodyPartType.UpperPart]].GetSprite();
            }

        }
    }


    public partial class UiPartParam
    {
        public BodyPartType tp;
    }
    public partial class UiPartModel
    {
        public BodyPartType tp;

    }
    public partial class UiPartCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.model.part = model.tp;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.tp = param.tp;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = TextManager.instance.GetTxt(model.tp.ToString());
            view.sta_.ChangeState(model.tp != parent.model.part ? 0 : 1);

        }
    }
    public partial class UiEquipPartParam
    {
        public EquipPartType tp;

    }
    public partial class UiEquipPartModel
    {
        public EquipPartType tp;
    }
    public partial class UiEquipPartCtrl
    {

        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                if (model.tp == parent.model.equipPart)
                {
                    parent.model.equipPart = EquipPartType.None;
                }
                else
                {
                    parent.model.equipPart = model.tp;
                }

                parent.Refresh();
            });
        }
        public override void OnShow()
        {
            model.tp = param.tp;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = TextManager.instance.GetTxt(model.tp.ToString());
            view.sta_.ChangeState(model.tp != parent.model.equipPart ? 0 : 1);
        }
    }

}