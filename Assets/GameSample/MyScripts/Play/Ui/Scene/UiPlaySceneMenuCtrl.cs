using Form;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Ui.Start;
using UnityEngine;
using Z_Language;
using Z_Map;
using Z_Text;
using Z_Ui;
using Z_Ui.Notify;
using Z_UnitSystem;
namespace Ui.PlaySceneMenu
{
    public partial class UiPlaySceneMenuModel
    {
        public float lastSaveTime;
    }
    public partial class UiPlaySceneMenuCtrl
    {
        public override void OnCreate()
        {
            view.btn_bbg.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_back.onClick.AddListener(() =>
            {
                Close();
            });
            view.btn_save.onClick.AddListener(() =>
            {
                Save();
            });
            view.btn_exit.onClick.AddListener(() =>
            {
                if (Time.time - model.lastSaveTime > 60&&!PlayManager.instance.boxPlay)
                {
                    NotifyManager.instance.AddPopup(
                        "", TextManager.instance.GetTxt("savePopupTitle"), true,
                        new List<string>() { TextManager.instance.GetTxt("yes"), TextManager.instance.GetTxt("no") },
                        new List<Func<bool>>() {
                            ()=>{
                    Save();
                    Exit();
                                return true;
                            },
                            () =>{
                    Exit();
                                return true;
                            }
                        });
                }
                else
                {
                    Exit();
                }

            });
        }
        public void Save()
        {
            if(PlayManager.instance.boxPlay)
            {
                NotifyManager.instance.AddTip(TextManager.instance.GetTxt("cantSaveWhenTest"));
                return;
            }
            model.lastSaveTime = Time.time;
            GameManager.instance.saveCtrl.SaveSaveStory(GameManager.instance.curStory.id);
        }
        public void Exit()
        {
            PlayManager.instance.Exit();
        }
    }
}
