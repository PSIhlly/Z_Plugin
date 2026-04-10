using Form;
using Ui.ParamShow;
using Ui.PlayData;
using Ui.PlaySceneMenu;
using Ui.Stick;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map.Analysis;
using Z_Ui;
using Z_Ui.Base;

namespace Ui.PlaySceneMain
{

    public partial class UiPlaySceneMainCtrl : IZ_Listener<StoryCharacterEvent>, IZ_Listener<CharacterSkillEvent>
    {
        Color[] colors = new Color[] { Color.red, Color.blue, Color.yellow };
        UiContainer<UiTeamerCtrl> teamerCon;
        UiContainer<UiParamShowCtrl> prmCon;

        UiStickParam eParam;
        UiStickParam qParam;
        public override void OnCreate()
        {
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
            view.btn_data.onClick.AddListener(() =>
            {
                if (GameManager.instance.curProgress.blockProgramUid != 0)
                {
                    UiManager.instance.ShowUi<UiPlayDataCtrl>();
                }
            });


            view.model_eStick.SetShow(true, eParam);
            view.model_qStick.SetShow(true, qParam);


            teamerCon = new UiContainer<UiTeamerCtrl>(view.go_teamer);
            prmCon = new UiContainer<UiParamShowCtrl>(view.model_ParamShow.gameObject);
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
            this.Register<StoryCharacterEvent>();
            this.Register<CharacterSkillEvent>();
            Refresh();
        }
        public override void OnHide()
        {
            this.Unregister<StoryCharacterEvent>();
            this.Unregister<CharacterSkillEvent>();
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
                            prmCon.Add(new UiParamShowParam() { max = prm.GetMax().num - prm.GetMin().num, value = prm.GetValue().num - prm.GetMin().num, color = colors[id] });
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
            con = new UiContainer<UiParamShowCtrl>(view.model_ParamShow.gameObject);
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
                            con.Add(new UiParamShowParam() { max = prm.GetMax().num - prm.GetMin().num, value = prm.GetValue().num - prm.GetMin().num, color = colors[id] });
                            if (id < colors.Length - 1)
                            {
                                id++;
                            }
                            break;
                    }
                }
            }
            con.Refresh();

            view.img_.sprite = TexAssetForm.DataByName.GetDk(model.data.avatarTexName, GlobalNameHelper.GetDefaultCharacterTexName()).GetSprite();

        }
    }

}
