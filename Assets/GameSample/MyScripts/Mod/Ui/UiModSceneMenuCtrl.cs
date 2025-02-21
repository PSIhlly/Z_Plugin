using Form;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Language;
using Z_Map;
using Z_Text;
using Z_Ui;
using Z_Ui.Notify;
using Z_UnitSystem;
namespace Ui
{
    public partial class UiModSceneMenuModel
    {
        public float lastSaveTime;
    }
    public partial class UiModSceneMenuCtrl
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
                if (Time.time - model.lastSaveTime > 60)
                {
                    NotifyManager.instance.AddPopup(
                        "",TextManager.instance.GetTxt("savePopupTitle"), true,
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
            model.lastSaveTime = Time.time;
            SaveAndLoad.Save(ModSceneManager.instance.fileName, JsonConvert.SerializeObject(MapManager.instance.dataCtrl.GetJsonData()));//先只存地图的

        }
        public void Exit()
        {
            ModSceneManager.instance.End();

        }
    }
}
