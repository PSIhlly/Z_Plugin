using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z_Ui.Base;
using Z_Texture;
using Z_String;
using Z_Math;
using Unity.VisualScripting;
using Z_DataSystem.Form;
using Z_Code.Form;
using Z_Text;

namespace Ui.ModStory.ModStorySkill.ModStorySkillUnit
{

    public partial class UiModStorySkillUnitParam
    {
        public SkillForm.Data data;
    }
    public partial class UiModStorySkillUnitModel
    {
        public SkillForm.Data data;
    }
    public partial class UiModStorySkillUnitCtrl
    {

        public override void OnCreate()
        {

            view.btn_back.onClick.AddListener(() =>
            {
                parent.SelPage(0);
            });
            view.ipt_name.onFinishInput = (s) =>
            {
                var lst = new List<string>();
                foreach (var data in SkillForm.DataByName.Values)
                    lst.Add(data.name);

                if (StringHelper.IsUniqueName(lst, s))
                    model.data.name = s;
                Refresh();
            };
            view.ipt_label.onFinishInput = (s) =>
            {
                model.data.label = s;
                Refresh();
            };
            view.ipt_cd.onFinishInput = (s) =>
            {
                model.data.cd = StringHelper.ToFloat(s,1);
                Refresh();
            };

            view.btn_icon.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportSkillIcon(parent.model.data.uid);
                Refresh();

            });

            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteSkill(parent.model.data.uid);
                parent.SelPage(0);
            });
            view.btn_e.onClick.AddListener(() =>
            {
                SetKillType(SkillType.E);
            });
            view.btn_q.onClick.AddListener(() =>
            {
                SetKillType(SkillType.Q);
            });
            view.btn_lightAttack.onClick.AddListener(() =>
            {
                SetKillType(SkillType.LightAttack);
            });
            view.btn_heavyAttack.onClick.AddListener(() =>
            {
                SetKillType(SkillType.HeavyAttack);
            });
            view.btn_onTriggerEvent.onClick.AddListener(() =>
            {
                var key = "onTriggerEvent";
                ModManager.instance.assetCtrl.ChooseEvent(model.data.events, key, EventType.Global, CmdTypeDataForm.defaultData.name, TextManager.instance.GetTxt(key), (item) =>
                {
                    Refresh();
                });
            });
            view.btn_triggerCondition.onClick.AddListener(() =>
            {
                var key = "triggerCondition";
                ModManager.instance.assetCtrl.ChooseTriggerCondition(TextManager.instance.GetTxt(key), (item) =>
                {
                    model.data.triggerConditionUid = item.id;
                    Refresh();
                });
            });
            

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
            view.ipt_name.Set(model.data.name);
            view.ipt_label.Set(model.data.label);
            view.ipt_cd.Set(model.data.cd.ToString());
            view.img_icon.sprite = TexAssetForm.DataByName[model.data.icon].sprite;
            view.sta_e.ChangeState(model.data.skillTypes.Contains(SkillType.E)?1:0);
            view.sta_q.ChangeState(model.data.skillTypes.Contains(SkillType.Q)?1:0);
            view.sta_lightAttack.ChangeState(model.data.skillTypes.Contains(SkillType.LightAttack)?1:0);
            view.sta_heavyAttack.ChangeState(model.data.skillTypes.Contains(SkillType.HeavyAttack)?1:0);
        }
        private void SetKillType(SkillType tp)
        {
           
                if (parent.model.data.skillTypes.Contains(tp))
                {
                    parent.model.data.skillTypes.Remove(tp);
                }
                else
                {
                    parent.model.data.skillTypes.Add(tp);
                }
                Refresh();
        }
    }

}