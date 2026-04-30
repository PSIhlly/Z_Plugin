using Form;
using System;
using Ui.AnimChoose;
using Z_DesignStyle;
using Z_Text;
using Z_Ui.Notify;

namespace Ui.ModStory.ModStoryCharacter.ModStoryCharacterUnit.ModStoryCharacterUnitConfig
{

    public partial class UiModStoryCharacterUnitConfigParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitConfigModel
    {

        public CharacterProductForm.Data data;
    }
    public partial class UiModStoryCharacterUnitConfigCtrl
    {
        public override void OnCreate()
        {

            view.btn_faceType.onClick.AddListener(() =>
            {
                var items = new EntryItem();
                foreach (FaceType tp in Enum.GetValues(typeof(FaceType)))
                {
                    items.Add(TextManager.instance.GetTxt(tp.ToString()), null, (int)tp);
                }
                NotifyManager.instance.AddChoose(TextManager.instance.GetTxt("Choose faceType"),
                    true, (item) =>
                    {
                        model.data.faceType = (FaceType)item.id;
                        Refresh();
                        return true;
                    }, items);

            });
           
            view.btn_moveSpeedParameter.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseCharacterParam(TextManager.instance.GetTxt("Choose Speed param"), (item) =>
                {
                    model.data.speedParamName = item.content;
                    Refresh();
                });
            });

            view.btn_unique.onClick.AddListener(() =>
            {
                model.data.unique = !model.data.unique;
                Refresh();
            });

            view.btn_lightAttack.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseSkill(TextManager.instance.GetTxt("Choose Skill"), (item) =>
                {
                    model.data.skill[SkillType.LightAttack] = item.uid;
                    Refresh();
                });
            });

            view.btn_heavyAttack.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseSkill(TextManager.instance.GetTxt("Choose Skill"), (item) =>
                {
                    model.data.skill[SkillType.HeavyAttack] = item.uid;
                    Refresh();
                });
            });

            view.btn_e.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseSkill(TextManager.instance.GetTxt("Choose Skill"), (item) =>
                {
                    model.data.skill[SkillType.E] = item.uid;
                    Refresh();
                });
            });

            view.btn_q.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseSkill(TextManager.instance.GetTxt("Choose Skill"), (item) =>
                {
                    model.data.skill[SkillType.Q] = item.uid;
                    Refresh();
                });
            });

            view.btn_passive1.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseSkill(TextManager.instance.GetTxt("Choose Skill"), (item) =>
                {
                    model.data.skill[SkillType.Passive] = item.uid;
                    Refresh();
                });
            });

            view.btn_passive2.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ChooseSkill(TextManager.instance.GetTxt("Choose Skill"), (item) =>
                {
                    model.data.skill[SkillType.Passive] = item.uid;
                    Refresh();
                });
            });


        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_moveSpeedParameter.text = model.data.speedParamName;

            view.txt_faceType.oriText = model.data.faceType.ToString();

            view.model_IdleAnimChoose.Set(new UiAnimChooseParam() { dic = model.data.defaultAnimName, options = model.data.animDic, key = "idle" });
            view.model_MoveAnimChoose.Set(new UiAnimChooseParam() { dic = model.data.defaultAnimName, options = model.data.animDic, key = "move" });


            view.sta_unique.ChangeState(model.data.unique ? 1 : 0);

            view.model_EventChooseCharacterTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterTouchEvent" });
            view.model_EventChooseCharacterLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onCharacterLeaveEvent" });
            view.model_EventChooseObjectTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectTouchEvent" });
            view.model_EventChooseObjectLeave.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onObjectLeaveEvent" });
            view.model_EventChooseShow.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onShowEvent" });
            view.model_EventChoosePerSecond.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onPerSecondEvent" });
            view.model_EventChooseBoundaryTouch.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onBoundaryTouchEvent" });
            view.model_EventChooseInteract.Set(new EventChoose.UiEventChooseParam() { dic = model.data.events, key = "onInteractEvent" });

            view.txt_lightAttack.text = model.data.skill.ContainsKey(SkillType.LightAttack) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.LightAttack]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.LightAttack]].name : "";

            view.txt_heavyAttack.text = model.data.skill.ContainsKey(SkillType.HeavyAttack) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.HeavyAttack]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.HeavyAttack]].name : "";

            view.txt_e.text = model.data.skill.ContainsKey(SkillType.E) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.E]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.E]].name : "";

            view.txt_q.text = model.data.skill.ContainsKey(SkillType.Q) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.Q]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.Q]].name : "";

            view.txt_passive1.text = model.data.skill.ContainsKey(SkillType.Passive) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.Passive]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.Passive]].name : "";

            view.txt_passive2.text = model.data.skill.ContainsKey(SkillType.Passive) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.Passive]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.Passive]].name : "";

            view.go_skills.SetActive(GameManager.instance.curProgress.enableSkill);


        }
    }

}