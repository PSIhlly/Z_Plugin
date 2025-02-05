using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Map;
using Z_UnitSystem;
namespace Ui
{
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
                SaveAndLoad.Save(ModSceneManager.instance.fileName, JsonConvert.SerializeObject(MapManager.instance.dataCtrl.GetJsonData()));//先只存地图的
            });
        }
    }
}
