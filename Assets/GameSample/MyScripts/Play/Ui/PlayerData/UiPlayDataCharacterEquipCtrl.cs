using Form;
using System;
using Z_Code;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Text;
using Z_Ui.Base;

namespace Ui.PlayDataCharacter.PlayDataCharacterEquip
{

    public partial class UiPlayDataCharacterEquipParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiPlayDataCharacterEquipModel
    {
        public CharacterProductForm.Data data;
        public EquipPartType selPart;
        public ItemProductForm.Data sel;
    }
    public partial class UiPlayDataCharacterEquipCtrl
    {

        UiScrViewContainer<UiGameEquipCtrl> gameEquipCon;
        UiScrViewContainer<UiGameArgsCtrl> gameArgsCon;
        public override void OnCreate()
        {

            gameEquipCon = new UiScrViewContainer<UiGameEquipCtrl>(this, view.go_gameEquip, view.scr_gameEquip);
            view.btn_unequip.onClick.AddListener(() =>
            {
                PlayManager.instance.infoCtrl.Unequip(model.data.uid, model.selPart);
            });
            view.btn_equip.onClick.AddListener(() =>
            {
                PlayManager.instance.infoCtrl.ChooseEquipItems(TextManager.instance.GetTxt("Equipment"), (it) =>
                {
                    PlayManager.instance.infoCtrl.Equip(model.data.uid, it.uid, model.selPart);
                }, model.selPart);
            });
            gameArgsCon = new UiScrViewContainer<UiGameArgsCtrl>(this, view.go_gameArgs, view.scr_gameArgs);

        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_show.ChangeState(model.selPart == EquipPartType.None ? 0 : 1);

            gameArgsCon.Clear();
            if (model.selPart != EquipPartType.None)
            {
                view.txt_part.text = TextManager.instance.GetTxt(model.selPart.ToString());
                view.txt_name.text = model.sel?.name;
                view.btn_equip.gameObject.SetActive(model.sel == null);
                view.btn_unequip.gameObject.SetActive(model.sel != null);
                if (model.sel != null)
                {
                    foreach (var pair in model.sel.paramDic)
                    {
                        if (model.data.CanShow(pair.Key))
                            gameArgsCon.Add(new UiGameArgsParam()
                            {
                                content = pair.Key + ":" + CodeHelper.GetBoxContent(pair.Value.GetValue()),
                            });
                    }
                }


            }
            gameArgsCon.Refresh();

            gameEquipCon.Clear();
            foreach (EquipPartType part in Enum.GetValues(typeof(EquipPartType)))
            {
                if (part != EquipPartType.None)
                    gameEquipCon.Add(new UiGameEquipParam()
                    {
                        part = part
                    });
            }
            gameEquipCon.Refresh();

        }
    }

    public partial class UiGameEquipParam
    {
        public EquipPartType part;
    }
    public partial class UiGameEquipModel
    {
        public EquipPartType part;
        public ItemProductForm.Data data;
    }
    public partial class UiGameEquipCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.model.sel = model.data;
                parent.model.selPart = model.part;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.part = param.part;
            model.data = ItemProductForm.DataByUid.GetDv(parent.model.data.equips.GetDv(model.part, 0), null);
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data != null ? 1 : 0);
            view.txt_.text = TextManager.instance.GetTxt(model.part.ToString());

        }
    }


    public partial class UiGameArgsParam
    {
        public string content;
    }
    public partial class UiGameArgsModel
    {
        public string content;

    }
    public partial class UiGameArgsCtrl
    {

        public override void OnCreate()
        {



        }
        public override void OnShow()
        {
            model.content = param.content;
            Refresh();
        }
        public void Refresh()
        {

            view.txt_.text = model.content;
        }
    }


}
