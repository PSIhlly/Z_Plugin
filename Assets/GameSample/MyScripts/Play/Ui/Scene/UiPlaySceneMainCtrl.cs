using Form;
using System.Collections.Generic;
using System.Linq;
using Ui.ParamShow;
using Ui.PlaySceneMenu;
using Ui.Stick;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Analysis;
using Z_Ui;
using Z_Ui.Base;
using Z_UnitSystem.Form;
using Ui.PlayDataBackpack;
using Ui.PlayDataCharacter;

namespace Ui.PlaySceneMain
{

    public partial class UiPlaySceneMainCtrl : IZ_Listener<StoryCharacterEvent>, IZ_Listener<CharacterSkillEvent>, IZ_Listener<SceneActionEvent>, IZ_Listener<MissionEvent>, IZ_Listener<StoryLifeEvent>
    {
        Color[] colors = new Color[] { Color.red, Color.blue, Color.yellow };
        UiContainer<UiTeamerCtrl> teamerCon;
        UiContainer<UiParamShowCtrl> prmCon;
        UiContainer<UiActionCtrl> actCon;

        UiStickParam eParam;
        UiStickParam qParam;

        List<int> actionUnitUids;
        public override void OnCreate()
        {
            actionUnitUids = new List<int>();
            void Drag(Vector3 dir, SkillType type)
            {
                if (!CanContinueSkill(type))
                    return;
                PlayManager.instance.sceneCtrl.SetCurOptSkill(type);
                dir = GetSkillDirection(dir);
                if (dir != Vector3.zero)
                    PlayManager.instance.sceneCtrl.SetPlayerRotation(dir, 360);
            }
            void Up(Vector3 dir, SkillType type)
            {
                bool canUse = CanContinueSkill(type);
                // Release our aim before UseSkill's synchronous UI refresh.
                Hide(type);
                if (!canUse)
                    return;
                var character = CharacterProductForm.DataByUid.GetDv(GameManager.instance.curProgress.characterUid, null);
                if (character != null && character.skill.ContainsKey(type))
                    PlayManager.instance.infoCtrl.UseSkill(character.uid, character.skill[type], false);
            }
            void Hide(SkillType type)
            {
                if (PlayManager.instance.sceneCtrl.GetCurOptSkill() == type)
                    PlayManager.instance.sceneCtrl.SetCurOptSkill(null);
            }
            ;
            eParam = new UiStickParam()
            {
                dragAct = (dir) =>
                {
                    Drag(dir, SkillType.E);
                },
                upAct = (dir) =>
                {
                    Up(dir, SkillType.E);
                },
                hideAct = () =>
                {
                    Hide(SkillType.E);
                },
                canStartFunc = () =>
                {
#if UNITY_STANDALONE_WIN
            return false;
#endif
                    return CanContinueSkill(SkillType.E) && PlayManager.instance.infoCtrl.CanUseSkill(GameManager.instance.curProgress.characterUid, SkillType.E);
                },
                canContinueFunc = () => CanContinueSkill(SkillType.E)
            };
            qParam = new UiStickParam()
            {
                dragAct = (dir) =>
                {
                    Drag(dir, SkillType.Q);
                },
                upAct = (dir) =>
                {
                    Up(dir, SkillType.Q);
                },
                hideAct = () =>
                {
                    Hide(SkillType.Q);
                },
                canStartFunc = () =>
                {
#if UNITY_STANDALONE_WIN
            return false;
#endif
                    return CanContinueSkill(SkillType.Q) && PlayManager.instance.infoCtrl.CanUseSkill(GameManager.instance.curProgress.characterUid, SkillType.Q);
                },
                canContinueFunc = () => CanContinueSkill(SkillType.Q)
            };

#if UNITY_STANDALONE_WIN
            view.page_PlayerTouchOpt.SetShow(false);
#else
            view.page_PlayerTouchOpt.SetShow(true);
#endif

            view.btn_menu.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiPlaySceneMenuCtrl>();
            });
            view.btn_dataBackpack.onClick.AddListener(() =>
            {
                if (GameManager.instance.curProgress.blockProgramUid == 0)
                {
                    UiManager.instance.ShowUi<UiPlayDataBackpackCtrl>();
                }
            });
            view.btn_dataCharacter.onClick.AddListener(() =>
            {
                if (GameManager.instance.curProgress.blockProgramUid == 0)
                {
                    UiManager.instance.ShowUi<UiPlayDataCharacterCtrl>();
                }
            });

            
            view.model_eStick.SetShow(true, eParam);
            view.model_qStick.SetShow(true, qParam);

            actCon = new UiContainer<UiActionCtrl>(this, view.go_action);
            teamerCon = new UiContainer<UiTeamerCtrl>(this, view.go_teamer);
            prmCon = new UiContainer<UiParamShowCtrl>(this, view.model_ParamShow.gameObject);
        }

        private bool CanContinueSkill(SkillType type)
        {
            var progress = GameManager.instance.curProgress;
            if (progress == null || !progress.enableSkill || progress.blockProgramUid != 0)
                return false;
            var current = PlayManager.instance.sceneCtrl.GetCurOptSkill();
            return current == null || current == type;
        }

        private static Vector3 GetSkillDirection(Vector3 dir)
        {
            if (dir.magnitude < 0.25f)
                return Vector3.zero;
            dir = dir.normalized;
            return new Vector3(dir.x, 0, dir.y);
        }

        private void RefreshSkillStick(UiStickCtrl stick, UiStickParam stickParam, SkillType type)
        {
            bool keepCaptured = stick.model.curId != -1 && stickParam.canContinueFunc != null && stickParam.canContinueFunc();
            var progress = GameManager.instance.curProgress;
            bool show = keepCaptured || (progress != null && progress.enableSkill && progress.blockProgramUid == 0 &&
                PlayManager.instance.infoCtrl.CanUseSkill(progress.characterUid, type));
            if (stick.active != show)
                stick.SetShow(show, stickParam);
        }

        public void OnEvent(StoryCharacterEvent evt)
        {
            if (active && (evt.type == StoryCharacterEventType.ChangeTeamer
                || evt.type == StoryCharacterEventType.ChangeCharacter
                || (evt.data != null && GameManager.instance.curProgress.teamActive.Contains(evt.data.uid))))
            {
                Refresh();
            }
        }
        public void OnEvent(CharacterSkillEvent evt)
        {
            Refresh();
        }
        public void OnEvent(MissionEvent evt)
        {
            if (active)
                view.page_PlaySceneMission.Refresh();
        }

        public void OnEvent(StoryLifeEvent evt)
        {
            if (active && evt.type == StoryLifeEventType.EverySecond)
                view.page_PlaySceneMission.RefreshDistance();
        }

        public override void OnShow()
        {
            actionUnitUids.Clear();
            this.Register<StoryCharacterEvent>();
            this.Register<CharacterSkillEvent>();
            this.Register<SceneActionEvent>();
            this.Register<MissionEvent>();
            this.Register<StoryLifeEvent>();
            
            Refresh();
        }
        public override void OnHide()
        {
            this.Unregister<StoryCharacterEvent>();
            this.Unregister<CharacterSkillEvent>();
            this.Unregister<SceneActionEvent>();
            this.Unregister<MissionEvent>();
            this.Unregister<StoryLifeEvent>();
        }


        public void Refresh()
        {
            teamerCon.Clear();
            foreach (var uid in GameManager.instance.curProgress.teamActive.OrderBy(uid => uid))
            {
                var teammate = CharacterProductForm.DataByUid.GetDv(uid, null);
                if (teammate != null)
                    teamerCon.Add(new UiTeamerParam() { data = teammate });
            }
            teamerCon.Refresh();

            int id = 0;
            var cur = CharacterProductForm.DataByUid.GetDv(GameManager.instance.curProgress.characterUid, null);
            prmCon.Clear();
            if (cur != null)
            {
                foreach (var prm in cur.paramDic.Values.OrderBy(data => data.uid))
                {
                    var protoPrm = CharacterParamForm.DataByName.GetDv(prm.name, null);
                    if (protoPrm != null)
                    {
                        switch (protoPrm.showType)
                        {
                            case ParamShowType.AlwaysWithPanel:
                            case ParamShowType.AlwaysWithPanelAndScene:
                            case ParamShowType.AlwaysWithPanelAndSceneWithoutPlayer:
                                var maxPrm = string.IsNullOrEmpty(prm.max) ? null : cur.paramDic.GetDv(prm.max, null);
                                float max = maxPrm == null ? GlobalSettings.MAX : maxPrm.GetValue().num;
                                var minPrm = string.IsNullOrEmpty(prm.min) ? null : cur.paramDic.GetDv(prm.min, null);
                                float min = minPrm == null ? 0 : minPrm.GetValue().num;
                                prmCon.Add(new UiParamShowParam() { max = max - min, value = prm.GetValue().num - min, color = colors[id] });
                                if (id < colors.Length - 1)
                                {
                                    id++;
                                }
                                break;
                        }
                    }
                }
            }
            prmCon.Refresh();

            view.go_noScene.SetActive((int)GameManager.instance.curProgress.editorStyle < 2);

            RefreshSkillStick(view.model_eStick, eParam, SkillType.E);
            RefreshSkillStick(view.model_qStick, qParam, SkillType.Q);

            actCon.Clear();
            if (cur != null)
            {
                foreach (var actUid in actionUnitUids.OrderBy(uid => uid))
                {
                    var data = UnitForm.DataByUid.GetDv(actUid, null);
                    if(data != null)
                    {
                        actCon.Add(new UiActionParam() { data = data });
                    }
                }
            }
            actCon.Refresh();

            view.page_PlaySceneMinimap.Refresh();
            view.page_PlaySceneMission.Refresh();
        }

        public void OnEvent(SceneActionEvent evt)
        {
            switch(evt.type)
            {
                case SceneActionEventType.Add:
                    if (actionUnitUids.Contains(evt.unitUid))
                        break;
                    actionUnitUids.Add(evt.unitUid);
                    Refresh();
                    break;
                case SceneActionEventType.Remove:
                    if (actionUnitUids.RemoveAll(uid => uid == evt.unitUid) > 0)
                    {
                        Refresh();
                    }
                    break;
            }
        }
    }
    public partial class UiTeamerParam
    {
        public CharacterProductForm.Data data;
    }
    public partial class UiTeamerModel
    {
        public CharacterProductForm.Data data;

    }
    public partial class UiTeamerCtrl
    {
        Color[] colors = new Color[] { Color.red, Color.blue, Color.yellow };
        UiContainer<UiParamShowCtrl> con;
        public override void OnCreate()
        {
            con = new UiContainer<UiParamShowCtrl>(this, view.model_ParamShow.gameObject);
            view.btn_.onClick.AddListener(() =>
            {
                PlayManager.instance.infoCtrl.ChooseCurrentCharacter(model.data.uid);
            });
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            view.txt_.text = model.data.name;

            con.Clear();
            int id = 0;
            foreach (var prm in model.data.paramDic.Values.OrderBy(data => data.uid))
            {
                var protoPrm = CharacterParamForm.DataByName.GetDv(prm.name, null);
                if (protoPrm != null)
                {
                    switch (protoPrm.showType)
                    {
                        case ParamShowType.AlwaysWithPanel:
                        case ParamShowType.AlwaysWithPanelAndScene:
                        case ParamShowType.AlwaysWithPanelAndSceneWithoutPlayer:
                            var maxPrm = string.IsNullOrEmpty(prm.max) ? null : model.data.paramDic.GetDv(prm.max, null);
                            float max = maxPrm==null? GlobalSettings.MAX: maxPrm.GetValue().num;
                            var minPrm = string.IsNullOrEmpty(prm.min) ? null : model.data.paramDic.GetDv(prm.min, null);
                            float min = minPrm == null ? 0 : minPrm.GetValue().num;
                            con.Add(new UiParamShowParam() { max = max - min, value = prm.GetValue().num - min, color = colors[id] });
                            if (id < colors.Length - 1)
                            {
                                id++;
                            }
                            break;
                    }
                }
            }
            con.Refresh();

            view.img_.BindTexData(TexAssetForm.DataById.GetDk(model.data.avatarTex, GlobalDefaultHelper.DefaultCharacterTexId));

        }
    }
    public partial class UiActionParam
    {
        public UnitForm.Data data;
    }
    public partial class UiActionModel
    {
        public UnitForm.Data data;
    }
    public partial class UiActionCtrl
    {
      
        public override void OnCreate()
        {
           view.btn_.onClick.AddListener(() =>
           {
               var player = PlayManager.instance.sceneCtrl.playerG;
               if (player == null)
                   return;
               var unit = model.data?.unit as MapUnit;
               if (unit == null)
                   return;
               string self = unit is CharacterUnit character
                   ? GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, character.productInfo.Item1.ToString())
                   : GlobalEventHelper.GetSceneObjectReference(unit);
               var heap = new Dictionary<string, BoxDataForm.Data>();
               heap["param1"] = CodeHelper.CreateBoxByStr(self);
               heap["param2"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, player.uid.ToString()));
               unit.ExecuteEvt("onInteractEvent", heap);
           });
        }
        public override void OnShow()
        {
            model.data = param.data;
            Refresh();
        }
        public void Refresh()
        {
            if(model.data.unit is CharacterUnit ch)
            {
                var pd = CharacterProductForm.DataByUid.GetDv(ch.productInfo.Item1, null);
                view.txt_.text = pd==null ? model.data.prefabName : pd.name;
            }
            else if (model.data.unit is ObjectUnit obj)
            {
                var pd = ObjectProductForm.GetProduct(obj.data);
                view.txt_.text = pd == null ? model.data.prefabName : pd.name;
            }
            else
            {
                view.txt_.text = model.data.prefabName;
            }
        }
    }

}
