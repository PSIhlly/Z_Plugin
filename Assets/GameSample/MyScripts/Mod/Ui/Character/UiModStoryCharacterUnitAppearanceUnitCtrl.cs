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
    public partial class UiModStoryCharacterUnitAppearanceUnitCtrl
    {

        UiScrViewContainer<UiItemCtrl> itemCon;
        UiScrViewContainer<UiPartCtrl> partCon;
        UiScrViewContainer<UiEquipPartCtrl> equipPartCon;
        public override void OnCreate()
        {

            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacterAnim(parent.model.data.name, model.data.name);
                parent.SelPage(0);
            });
            view.ipt_name.onEndEdit.AddListener((s) =>
            {
                if (StringHelper.IsUniqueName(parent.model.data.animDic.Keys, s))
                    ModManager.instance.assetCtrl.RenameCharacterAnim(parent.model.data.name, model.data.name, s);
                Refresh();
            });
            view.ipt_scale.onEndEdit.AddListener((s) =>
            {
                model.data.scale = StringHelper.ToFloat(s, 1);
                Refresh();
            });
            view.ipt_interval.onEndEdit.AddListener((s) =>
            {
                model.data.animTimeInterval = StringHelper.ToFloat(s, 0.2f);
                Refresh();
            });

            view.btn_addTex.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.CreateCharacterAnimId(parent.model.data.name, model.data.name);
                Refresh();
            });
            view.btn_deleteTex.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteCharacterAnimId(parent.model.data.name, model.data.name,model.id);
                Refresh();
            });
            view.btn_resetTex.onClick.AddListener(() =>
            {
                model.data.animClip[model.id].partTex[model.part] = "";
                Refresh();
            });
            view.btn_image.onClick.AddListener(() =>
            {
                if (model.equipPart == EquipPartType.None)
                {
                    ModManager.instance.assetCtrl.ImportCharacterAnim(parent.model.data.name, model.data.name, model.part, model.id);
                    Refresh();
                }
            });
            itemCon = new UiScrViewContainer<UiItemCtrl>(view.go_item, view.scr_items);
            

            partCon = new UiScrViewContainer<UiPartCtrl>(view.go_part, view.scr_parts);
            equipPartCon = new UiScrViewContainer<UiEquipPartCtrl>(view.go_equipPart, view.scr_equipParts);
            view.btn_plus.onClick.AddListener(() =>
            {
                var o = model.data.animClip[model.id].equipTrs[model.equipPart];
                model.data.animClip[model.id].equipTrs[model.equipPart] = (o.Item1+1, o.Item2, o.Item3,o.Item4);
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
                var lst=new List<(string, Sprite)>();
                foreach(var e in Enum.GetValues(typeof(ItemStyle)))
                {
                    lst.Add(((string)e,null));
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose main character"),
                   true, (id) =>
                   {

                       model.data.animClip[model.id].equipStyle[model.equipPart] = (ItemStyle)id;
                       Refresh();
                       return true;
                   }, lst);
            });
            view.btn_enablePart.onClick.AddListener(() => {
                model.data.animClip[model.id].partEnable[model.part] = !model.data.animClip[model.id].partEnable[model.part];
                Refresh();
             });
        }
        public override void OnShow()
        {
            DisplayCameraAreaManager.instance.Show();
            model.id = -1;
            model.part = BodyPartType.UpperPart;
            model.equipPart = EquipPartType.None;
            Refresh();
        }
        public override void OnDisable()
        {
            DisplayCameraAreaManager.instance.Hide();
        }
        public void Refresh()
        {
            view.ipt_name.Set(model.data.name);
            view.ipt_scale.Set(model.data.scale.ToString());
            view.ipt_interval.Set(model.data.animTimeInterval.ToString());

            view.sta_show.ChangeState(model.id == -1 ? 0 : 1);
            view.sta_equip.ChangeState(model.equipPart ==  EquipPartType.None ? 0 : 1);

            equipPartCon.Clear();
            partCon.Clear();

            DisplayCameraAreaManager.instance.Clear();
            if (model.id > -1)
            {
                view.sta_enablePart.ChangeState(model.data.animClip[model.id].partEnable[model.part]?1:0);

                view.sta_equip.ChangeState(0);

                foreach(EquipPartType equipPart in Enum.GetValues(typeof(EquipPartType)))
                {
                    equipPartCon.Add(new UiEquipPartParam()
                    {
                        tp= equipPart
                    });
                }

                foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
                {
                    partCon.Add(new UiPartParam()
                    {
                        tp= part
                    });
                }

                if(model.equipPart != EquipPartType.None)
                {
                    view.txt_layer.text = model.data.animClip[model.id].equipTrs[model.equipPart].Item3.ToString();
                    
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
                    id=i
                });
            }
            itemCon.Add(new UiItemParam()
            {
                id=-1
            });
            itemCon.Refresh();


        }
        private void RefreshView()
        {
            List<string> texNameLst = new List<string>() {
                model.data.animClip[model.id].partTex[BodyPartType.UpperPart],
                model.data.animClip[model.id].partTex[BodyPartType.LowerPart],
                ""
                };
            var showGo = GameManager.instance.utilCtrl.CombineNewCharacterByPrefabs("fakeChara", texNameLst, false);
            showGo.SetActive(true);
            DisplayCameraAreaManager.instance.Add(showGo, Vector3.zero);
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
                ModManager.instance.assetCtrl.CreateCharacterAnimId(parent.parent.model.data.name, parent.model.data.name);
                parent.Refresh();
            });
            view.btn_.onClick.AddListener(() =>
            {
                parent.model.id = model.id;
            });

        }
        public override void OnShow()
        {
            model.id=param.id;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_item.ChangeState(model.id == -1 ? 0 : 1);
            if(model.id!=-1)
            {
                view.txt_.text = "";
                view.img_.sprite = StoryTexAssetForm.DataByName[parent.model.data.animClip[model.id].partTex[BodyPartType.UpperPart]].sprite;
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

            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text=model.tp.ToString();
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
            view.txt_.text = model.tp.ToString();
            view.sta_.ChangeState(model.tp!=parent.model.equipPart?0:1);
        }
    }

}