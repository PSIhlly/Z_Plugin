using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Ui.EventChoose;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Math;
using Z_String;
using Z_Text;
using Z_Texture;
using Z_Ui.Base;
using Z_DataSystem;

namespace Ui.ModStory.ModStorySkill.ModStorySkillUnit.ModStorySkillUnitOverview
{

    public partial class UiModStorySkillUnitOverviewParam
    {
        public SkillProductForm.Data data;
    }
    public partial class UiModStorySkillUnitOverviewModel
    {
        public SkillProductForm.Data data;

    }
    public partial class UiModStorySkillUnitOverviewCtrl : IZ_Listener<AssetEvent>
    {

        public override void OnCreate()
        {
            Z_EventHelper.Register(this);
            view.btn_delete.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.DeleteSkill(parent.model.data.uid);
                parent.parent.SelPage(0);

            });
            view.ipt_name.onFinishInput += (s) =>
            {
                var lst = new List<string>();
                foreach (var data in SkillProductForm.DataByNameProtouid.Values)
                    lst.Add(data.name);

                if (StringHelper.IsUniqueName(lst, s))
                    model.data.name = s;
            };
            view.ipt_label.onFinishInput += (s) =>
            {
                model.data.label = s;
            };
            view.btn_icon.onClick.AddListener(() =>
            {
                ModManager.instance.assetCtrl.ImportSkillIcon(model.data.uid);
            });
            view.ipt_cd.onFinishInput += (s) =>
            {
                model.data.cd = StringHelper.ToFloat(s, 1);
                Refresh();
            };
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

        public void OnEvent(AssetEvent evt)
        {
            if (active)
                Refresh();
        }

        public override void OnShow()
        {
            if (param != null)
                model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.ipt_name.Set(model.data.name);
            view.ipt_label.Set(model.data.label);
            view.ipt_cd.Set(model.data.cd.ToString());
            view.img_icon.sprite = TexAssetForm.DataByName[model.data.icon].GetSprite();
            view.sta_e.ChangeState(model.data.skillTypes.Contains(SkillType.E) ? 1 : 0);
            view.sta_q.ChangeState(model.data.skillTypes.Contains(SkillType.Q) ? 1 : 0);
            view.sta_lightAttack.ChangeState(model.data.skillTypes.Contains(SkillType.LightAttack) ? 1 : 0);
            view.sta_heavyAttack.ChangeState(model.data.skillTypes.Contains(SkillType.HeavyAttack) ? 1 : 0);

            view.model_EventChoose.Set(new UiEventChooseParam() { dic = model.data.events, key = "invoke" });

        }

        private void SetKillType(SkillType tp)
        {
            if (model.data.skillTypes.Contains(tp))
            {
                model.data.skillTypes.Remove(tp);
            }
            else
            {
                model.data.skillTypes.Add(tp);
            }
            Refresh();
        }
    }

}
