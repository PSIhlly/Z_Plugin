using Form;
using System.Collections.Generic;
using Ui.ParamShow;
using Ui.PlaySceneMenu;
using Ui.Stick;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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

    public partial class UiPlaySceneMainCtrl : IZ_Listener<StoryCharacterEvent>, IZ_Listener<CharacterSkillEvent>,IZ_Listener<SceneActionEvent>
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
                PlayManager.instance.sceneCtrl.SetPlayerRotation(dir, 360);
                PlayManager.instance.sceneCtrl.SetCurOptSkill(SkillType.Q);
            }
            void Up(Vector3 dir, SkillType type)
            {
                var character = CharacterProductForm.DataByUid.GetDv(GameManager.instance.curProgress.characterUid, null);
                if (character != null && character.skill.ContainsKey(type))
                    PlayManager.instance.infoCtrl.UseSkill(character.uid, character.skill[type], false);
                PlayManager.instance.sceneCtrl.SetCurOptSkill(null);
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
                    return PlayManager.instance.infoCtrl.CanUseSkill(GameManager.instance.curProgress.characterUid, SkillType.E);
                }
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
                    return PlayManager.instance.infoCtrl.CanUseSkill(GameManager.instance.curProgress.characterUid, SkillType.Q);
                }
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

        public void OnEvent(StoryCharacterEvent evt)
        {
            if (GameManager.instance.curProgress.teamActive.Contains(evt.data.uid))
            {
                Refresh();
            }
        }
        public void OnEvent(CharacterSkillEvent evt)
        {
            Refresh();
        }

        public override void OnShow()
        {
            actionUnitUids.Clear();
            this.Register<StoryCharacterEvent>();
            this.Register<CharacterSkillEvent>();
            this.Register<SceneActionEvent>();
            
            Refresh();
        }
        public override void OnHide()
        {
            this.Unregister<StoryCharacterEvent>();
            this.Unregister<CharacterSkillEvent>();
            this.Unregister<SceneActionEvent>();
        }


        public void Refresh()
        {
            teamerCon.Clear();
            foreach (var uid in GameManager.instance.curProgress.teamActive)
            {
                teamerCon.Add(new UiTeamerParam()
                {
                    data = CharacterProductForm.DataByUid[uid]
                });
            }
            teamerCon.Refresh();

            int id = 0;
            var cur = CharacterProductForm.DataByUid[GameManager.instance.curProgress.characterUid];
            prmCon.Clear();
            foreach (var prm in cur.paramDic.Values)
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
            prmCon.Refresh();

            view.go_noScene.SetActive((int)GameManager.instance.curProgress.editorStyle < 2);

            if (view.model_eStick.active != PlayManager.instance.infoCtrl.CanUseSkill(GameManager.instance.curProgress.characterUid, SkillType.E))
                view.model_eStick.SetShow(!view.model_eStick.active, eParam);

            if (view.model_qStick.active != PlayManager.instance.infoCtrl.CanUseSkill(GameManager.instance.curProgress.characterUid, SkillType.Q))
                view.model_qStick.SetShow(!view.model_qStick.active, qParam);

            actCon.Clear();
            foreach (var actUid in actionUnitUids)
            {
                var data = UnitForm.DataByUid.GetDv(actUid, null);
                if(data != null)
                {
                    actCon.Add(new UiActionParam() { data = data });
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
                    actionUnitUids.Add(evt.unitUid);
                    Refresh();
                    break;
                case SceneActionEventType.Remove:
                    if(actionUnitUids.Contains(evt.unitUid))
                    {
                        actionUnitUids.Remove(evt.unitUid);
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
            foreach (var prm in model.data.paramDic.Values)
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
               var heap = new Dictionary<string, BoxDataForm.Data>();
               heap["self"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, ((MapUnit)model.data.unit).productInfo.Item1.ToString()));
               heap["target"] = CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, PlayManager.instance.sceneCtrl.playerG.uid.ToString()));
               ((MapUnit)model.data.unit).ExecuteEvt("onInteractEvent", null);
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
                var pd = MapObjectForm.DataById.GetDv(obj.productInfo.Item1,null);
                view.txt_.text = pd == null ? model.data.prefabName : pd.name;
            }
            else
            {
                view.txt_.text = model.data.prefabName;
            }
        }
    }

}
