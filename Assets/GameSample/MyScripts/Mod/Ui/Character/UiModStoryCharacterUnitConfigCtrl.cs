using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using Ui.AnimChoose;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Text;
using Z_Ui.Notify;
using Z_Ui.Base;

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
        private UiContainer<UiPassTypeCtrl> passTypeCon;

        private EventChoose.UiSceneEventChooseList eventChooseList;
        private TriggerEventChoose.GlobalVariableTriggerList globalTriggers;

        public override void OnCreate()
        {
            eventChooseList = new EventChoose.UiSceneEventChooseList(this, view.go_eventChoose);
            globalTriggers = new TriggerEventChoose.GlobalVariableTriggerList(this, view.go_triggerEventChoose);
            passTypeCon = new UiContainer<UiPassTypeCtrl>(this, view.go_passType);
            view.ipt_size.contentType = TMPro.TMP_InputField.ContentType.IntegerNumber;
            view.ipt_size.onFinishInput += value =>
            {
                int oldSize = Math.Max(1, model.data.size);
                if (int.TryParse(value, out int size))
                    model.data.size = Math.Max(1, size);
                else
                    model.data.size = oldSize;
                view.ipt_size.Set(model.data.size.ToString(), false);
            };

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

            view.btn_minimapIcon.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportCharacterMinimap(model.data.uid); 
                Refresh();
            });
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public override void OnHide()
        {
            eventChooseList?.Clear();
            globalTriggers?.Clear();
        }

        public void Refresh()
        {
            if (model.data.passType == null)
                model.data.passType = new List<int>();
            model.data.passType.RemoveAll(id => id == 0 || !PassTypeForm.DataById.ContainsKey(id));
            model.data.passType = model.data.passType.Distinct().ToList();

            passTypeCon.Clear();
            foreach (var passType in PassTypeForm.DataById.Values.OrderBy(data => data.id))
                passTypeCon.Add(new UiPassTypeParam { id = passType.id });
            passTypeCon.Refresh();
            view.go_passTypes.SetActive(PassTypeForm.DataById.Count > 0);

            if (model.data.size < 1)
                model.data.size = 1;
            view.ipt_size.Set(model.data.size.ToString());
            view.txt_moveSpeedParameter.text = model.data.speedParamName;

            view.txt_faceType.oriText = model.data.faceType.ToString();

            view.model_IdleAnimChoose.Set(new UiAnimChooseParam() { dic = model.data.defaultAnimName, options = model.data.animDic, key = "idle" });
            view.model_MoveAnimChoose.Set(new UiAnimChooseParam() { dic = model.data.defaultAnimName, options = model.data.animDic, key = "move" });


            view.sta_unique.ChangeState(model.data.unique ? 1 : 0);

            eventChooseList.Render(model.data.events, GameManager.instance.curProgress.enableMinimap);
            var data = model.data;
            var events = data.events;
            globalTriggers.Render(events, () => ReferenceEquals(model.data, data) && ReferenceEquals(data.events, events)
                && CharacterProductForm.DataByUid.TryGetValue(data.uid, out var current) && ReferenceEquals(current, data));

            view.txt_lightAttack.text = model.data.skill.ContainsKey(SkillType.LightAttack) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.LightAttack]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.LightAttack]].name : "";

            view.txt_heavyAttack.text = model.data.skill.ContainsKey(SkillType.HeavyAttack) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.HeavyAttack]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.HeavyAttack]].name : "";

            view.txt_e.text = model.data.skill.ContainsKey(SkillType.E) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.E]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.E]].name : "";

            view.txt_q.text = model.data.skill.ContainsKey(SkillType.Q) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.Q]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.Q]].name : "";

            view.txt_passive1.text = model.data.skill.ContainsKey(SkillType.Passive) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.Passive]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.Passive]].name : "";

            view.txt_passive2.text = model.data.skill.ContainsKey(SkillType.Passive) && SkillProductForm.DataByUid.ContainsKey(model.data.skill[SkillType.Passive]) ? SkillProductForm.DataByUid[model.data.skill[SkillType.Passive]].name : "";

            view.go_skills.SetActive(GameManager.instance.curProgress.enableSkill);

            view.go_minimap.SetActive(GameManager.instance.curProgress.enableMinimap);
            view.img_minimapIcon.BindTexData(TexAssetForm.DataById.GetDv(model.data.minimapIcon, TexAssetForm.DataById[GlobalDefaultHelper.DefaultStoryTexId]));
            Z_Ui.UiManager.Rebuild(gameObject, true);
        }
    }

    public partial class UiPassTypeParam
    {
        public int id;
    }

    public partial class UiPassTypeModel
    {
        public int id;
    }

    public partial class UiPassTypeCtrl
    {
        public override void OnCreate()
        {
            view.btn_.onClick.AddListener(() =>
            {
                var selected = parent.model.data.passType;
                if (selected.Contains(model.id))
                    selected.Remove(model.id);
                else
                    selected.Add(model.id);
                parent.model.data.passType = selected;
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
            if (!PassTypeForm.DataById.TryGetValue(model.id, out var passType))
                return;
            view.txt_.text = passType.name;
            view.sta_.ChangeState(parent.model.data.passType.Contains(model.id) ? 1 : 0);
        }
    }

}
