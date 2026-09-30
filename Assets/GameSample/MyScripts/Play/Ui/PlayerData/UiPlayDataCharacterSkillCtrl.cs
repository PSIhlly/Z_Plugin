using Form;
using System;
using System.Collections.Generic;
using Z_Code;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Text;
using Z_Ui.Base;

namespace Ui.PlayDataCharacter.PlayDataCharacterSkill
{

    public partial class UiPlayDataCharacterSkillParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiPlayDataCharacterSkillModel
    {
        public CharacterProductForm.Data data;
        public SkillType selSkill;
        public SkillProductForm.Data sel;
        public bool hasSelection;
    }
    public partial class UiPlayDataCharacterSkillCtrl
    {

        UiContainer<UiGameEquipCtrl> gameSkillCon;
        UiScrViewContainer<UiGameArgsCtrl> gameArgsCon;
        public override void OnCreate()
        {

            gameSkillCon = new UiContainer<UiGameEquipCtrl>(this, view.go_gameEquip);
            gameArgsCon = new UiScrViewContainer<UiGameArgsCtrl>(this, view.go_gameArgs, view.scr_gameArgs);

        }
        public override void OnShow()
        {
            model.data = param?.data;
            Refresh();
        }
        public void Refresh()
        {
            model.sel = null;
            if (model.hasSelection && model.data?.skill != null &&
                model.data.skill.TryGetValue(model.selSkill, out var selectedUid))
                SkillProductForm.DataByUid.TryGetValue(selectedUid, out model.sel);
            if (model.sel == null)
                model.hasSelection = false;

            view.txt_.text = model.hasSelection ? TextManager.instance.GetTxt(model.selSkill.ToString()) : string.Empty;
            view.txt_name.text = model.sel?.name ?? string.Empty;
            view.txt_desc.text = model.sel?.desc ?? string.Empty;

            gameArgsCon.Clear();
            if (model.sel != null)
            {
                foreach (var pair in model.sel.paramDic)
                {
                    gameArgsCon.Add(new UiGameArgsParam()
                    {
                        content = pair.Key + ":" + CodeHelper.GetBoxContent(pair.Value.GetValue()),
                    });
                }
            }
            gameArgsCon.Refresh();

            gameSkillCon.Clear();
            if (model.data?.skill != null)
            {
                foreach (SkillType type in Enum.GetValues(typeof(SkillType)))
                {
                    if (!model.data.skill.TryGetValue(type, out var uid) ||
                        !SkillProductForm.DataByUid.TryGetValue(uid, out var skill))
                        continue;
                    gameSkillCon.Add(new UiGameEquipParam()
                    {
                        part = type,
                        data = skill
                    });
                }
            }
            gameSkillCon.Refresh();

        }
    }

    public partial class UiGameEquipParam
    {
        public SkillType part;
        public SkillProductForm.Data data;
    }
    public partial class UiGameEquipModel
    {
        public SkillType part;
        public SkillProductForm.Data data;
    }
    public partial class UiGameEquipCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.model.hasSelection = true;
                parent.model.selSkill = model.part;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.part = param.part;
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.sta_exist.ChangeState(model.data != null ? 1 : 0);
            if (model.data != null)
            {
                view.img_.BindTexData(TexAssetForm.DataById[model.data.icon]);
            }
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
