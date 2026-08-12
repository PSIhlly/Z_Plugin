using Form;
using Item;
using System;
using System.Collections;
using System.Collections.Generic;
using Ui.Loading;
using Ui.Mod;
using Ui.ModStory.ModStoryMission;
using Ui.ModStoryEditorStyleWindow;
using UnityEngine;
using Z_DataSystem;
using Z_Text;
using Z_Texture;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;

namespace Ui.ModStory
{
    public partial class UiModStoryModel
    {
        public UiCtrl curUi;
    }
    public partial class UiModStoryCtrl
    {

        public override void OnCreate()
        {
            view.btn_back.onClick.AddListener(() =>
            {
                Close();
                Main2StoryManager.instance.UnloadStoryUgc();
                UiManager.instance.ShowUi<UiModCtrl>();
            });
            view.btn_play.onClick.AddListener(() =>
            {
                GameManager.instance.saveCtrl.SaveCoreStory(GameManager.instance.curStory.id);
                Close();
                int curId = GameManager.instance.curStory.id;
                Main2StoryManager.instance.UnloadStoryUgc();
                Main2StoryManager.instance.StartLoadStoryPlay(curId, true);

            });
            view.btn_save.onClick.AddListener(() =>
            {
                GameManager.instance.saveCtrl.SaveCoreStory(GameManager.instance.curStory.id);
                NotifyManager.instance.AddTip(TextManager.instance.GetTxt("save success"));
            });
            view.btn_modCmd.onClick.AddListener(() =>
            {
                NotifyManager.instance.AddInputArea("ModCmd", true, command =>
                {
                    if (string.IsNullOrWhiteSpace(command))
                        return false;

                    bool success = ModCmd.TryExecute(command.Trim(), ModCmdScope.Story, out string result);
                    NotifyManager.instance.AddTip(result);
                    if (success)
                        Refresh();
                    return success;
                });
            });
            view.btn_delete.onClick.AddListener(() =>
            {
                NotifyManager.instance.AddPopup(TextManager.instance.GetTxt("delete"), TextManager.instance.GetTxt("delete confirm"), true,
                    new List<string>() { TextManager.instance.GetTxt("yes"), TextManager.instance.GetTxt("no") },
                    new List<Func<bool>>()
                    {
                        ()=>
                        {
                            int id = GameManager.instance.curStory.id;
                            Close();
                            Main2StoryManager.instance.UnloadStoryUgc();

                            Main2StoryManager.instance.DeleteStory(id);
                            UiManager.instance.ShowUi<UiModCtrl>();
                            return true;
                        },
                        ()=>
                        {
                            return true;
                        }
                    }, true);

            });
            view.btn_check.onClick.AddListener(() =>
            {
                NotifyManager.instance.AddPopup(TextManager.instance.GetTxt("check"), ModManager.instance.checkCtrl.Check(), true, enableScr: true);
            });
            view.btn_outPut.onClick.AddListener(() =>
            {
                NativeGallery.SaveImageToGallery(GameManager.instance.saveCtrl.Package(GameManager.instance.curStory.id), "GameMod", GameManager.instance.curStory.name + ".png", (su, p) =>
                {
                    NotifyManager.instance.AddTip(TextManager.instance.GetTxt("save success"));
                });
            });

            view.btn_overview.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryOverview;
                Refresh();
            });
            view.btn_parameter.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryParameter;
                Refresh();
            });
            view.btn_character.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryCharacter;
                Refresh();
            });
            view.btn_skill.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStorySkill;
                Refresh();
            });
            view.btn_item.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryItem;
                Refresh();
            });
            view.btn_mapObject.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryMapObject;
                Refresh();
            });
            view.btn_effect.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryEffect;
                Refresh();
            });
            view.btn_event.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryEvent;
                Refresh();
            });
            view.btn_map.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryMap;
                Refresh();
            });
            view.btn_mission.onClick.AddListener(() =>
            {
                model.curUi = view.page_ModStoryMission;
                Refresh();
            });
            view.btn_style.onClick.AddListener(() =>
            {
                UiManager.instance.ShowUi<UiModStoryEditorStyleWindowCtrl>(new UiModStoryEditorStyleWindowParam()
                {
                    onSelect = (type) =>
                    {
                        GameManager.instance.curProgress.editorStyle = type;
                        Refresh();
                    }
                });
            });
        }

        public override void OnShow()
        {
            Refresh();
        }
        public void Refresh()
        {
            view.txt_style.oriText = GameManager.instance.curProgress.editorStyle.ToString();

            view.page_ModStoryOverview.SetShow(model.curUi == view.page_ModStoryOverview);
            view.sta_overview.ChangeState(model.curUi == view.page_ModStoryOverview ? 1 : 0);

            view.btn_parameter.gameObject.SetActive((int)GameManager.instance.curProgress.editorStyle >= 2);
            view.page_ModStoryParameter.SetShow(model.curUi == view.page_ModStoryParameter);
            view.sta_parameter.ChangeState(model.curUi == view.page_ModStoryParameter ? 1 : 0);

            view.btn_character.gameObject.SetActive((int)GameManager.instance.curProgress.editorStyle >= 2);
            view.page_ModStoryCharacter.SetShow(model.curUi == view.page_ModStoryCharacter);
            view.sta_character.ChangeState(model.curUi == view.page_ModStoryCharacter ? 1 : 0);

            view.btn_skill.gameObject.SetActive((int)GameManager.instance.curProgress.editorStyle >= 3 && GameManager.instance.curProgress.enableSkill);
            view.page_ModStorySkill.SetShow(model.curUi == view.page_ModStorySkill);
            view.sta_skill.ChangeState(model.curUi == view.page_ModStorySkill ? 1 : 0);

            view.btn_item.gameObject.SetActive((int)GameManager.instance.curProgress.editorStyle >= 2);
            view.page_ModStoryItem.SetShow(model.curUi == view.page_ModStoryItem);
            view.sta_item.ChangeState(model.curUi == view.page_ModStoryItem ? 1 : 0);

            view.btn_mapObject.gameObject.SetActive((int)GameManager.instance.curProgress.editorStyle >= 2);
            view.page_ModStoryMapObject.SetShow(model.curUi == view.page_ModStoryMapObject);
            view.sta_mapObject.ChangeState(model.curUi == view.page_ModStoryMapObject ? 1 : 0);

            view.btn_effect.gameObject.SetActive((int)GameManager.instance.curProgress.editorStyle >= 3);
            view.page_ModStoryEffect.SetShow(model.curUi == view.page_ModStoryEffect);
            view.sta_effect.ChangeState(model.curUi == view.page_ModStoryEffect ? 1 : 0);

            view.btn_mission.gameObject.SetActive((int)GameManager.instance.curProgress.editorStyle >= 2 && GameManager.instance.curProgress.enableMission);
            view.page_ModStoryMission.SetShow(model.curUi == view.page_ModStoryMission);
            view.sta_mission.ChangeState(model.curUi == view.page_ModStoryMission ? 1 : 0);

            view.page_ModStoryEvent.SetShow(model.curUi == view.page_ModStoryEvent);
            view.sta_event.ChangeState(model.curUi == view.page_ModStoryEvent ? 1 : 0);

            view.btn_map.gameObject.SetActive((int)GameManager.instance.curProgress.editorStyle >= 2);
            view.page_ModStoryMap.SetShow(model.curUi == view.page_ModStoryMap);
            view.sta_map.ChangeState(model.curUi == view.page_ModStoryMap ? 1 : 0);

        }

    }

}
