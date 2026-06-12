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
    }
    public partial class UiPlayDataCharacterSkillCtrl
    {

        UiScrViewContainer<UiGameSkillCtrl> gameSkillCon;
        UiScrViewContainer<UiGameArgsCtrl> gameArgsCon;
        public override void OnCreate()
        {

            gameSkillCon = new UiScrViewContainer<UiGameSkillCtrl>(this, view.go_gameSkill, view.scr_gameSkill);
            gameArgsCon = new UiScrViewContainer<UiGameArgsCtrl>(this, view.go_gameArgs, view.scr_gameArgs);

        }
        public override void OnShow()
        {
            if (param != null)
            {
                model.data = param.data;
            }
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = model.selSkill != SkillType.LightAttack ? TextManager.instance.GetTxt(model.selSkill.ToString()) : "";
            view.txt_name.text = model.sel?.name;

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
            foreach (SkillType type in Enum.GetValues(typeof(SkillType)))
            {
                gameSkillCon.Add(new UiGameSkillParam()
                {
                    part = type
                });
            }
            gameSkillCon.Refresh();

        }
    }

    public partial class UiGameSkillParam
    {
        public SkillType part;
    }
    public partial class UiGameSkillModel
    {
        public SkillType part;
        public SkillProductForm.Data data;
    }
    public partial class UiGameSkillCtrl
    {

        public override void OnCreate()
        {

            view.btn_.onClick.AddListener(() =>
            {
                parent.model.sel = model.data;
                parent.model.selSkill = model.part;
                parent.Refresh();
            });

        }
        public override void OnShow()
        {
            model.part = param.part;
            model.data = SkillProductForm.DataByUid.GetDv(parent.model.data.skill.GetDv(model.part, 0), null);
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
