using Form;
using Item;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Ui;
using Ui.EnterMain;
using Ui.ModStory.ModStoryEffect.ModStoryEffectUnit;
using UnityEngine;
using UnityEngine.Rendering;
using Z_ByteSerialize;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Input;
using Z_Language;
using Z_Map;
using Z_Texture;
using Z_Ui;
using Z_UnitSystem;

public class CameraMoveEvent : Z_Event
{

}

public static partial class GlobalSettings
{
    public static float DRAG_DIS2 => InputManager.instance.screenSize.x / 25;
    public static int TERRAIN_LAYER_MAX => 3;
    public static int TEXTURE_MAX = 100000;
    public static int TEXTURE_MASK_MAX = 100000;
    public static int OBJECT_MAX = 100000;
}
public static class GlobalNameHelper
{
    public static string GetInternalPrefabName(string name) => Z_Map.GlobalHelper.GetInternalPrefabName(name);
    public static string GetRuntimePrefabName(string name="") => "runtime$" + name;
    public static string GetDefaultTexName(string name="") => "reserved$"+ name;
    public static bool IsInnerAssetName(string name)
    {
        return name.Contains("$");
    }

    public static string GetDefaultStoryTexName() => GetDefaultTexName("story");
    public static string GetDefaultCharacterTexName() => GetDefaultTexName("character");
    public static string GetDefaultItemTexName() => GetDefaultTexName("item");
    public static string GetDefaultModelTexName() => GetDefaultTexName("model");
}


public class GameManager : Z_MonoManager<GameManager>
{
    public GameUtilController utilCtrl;
    public GameSaveController saveCtrl;
    public GameEventController evtCtrl;
    public GameMapController mapCtrl; 
    public GameCharacterController characterCtrl;
    public GameItemController itemCtrl;
    public GameEffectController effectCtrl;


    public StoryForm.Data curStory;
    public SceneForm.Data curScene;
    public ConfigForm.Data curConfig => ConfigForm.DataByUid[1];

    public override void Init()
    {
        base.Init();
        LanguageManager.instance.SetLanguage(Language.Cn);

        utilCtrl = new GameUtilController(this);
        saveCtrl = new GameSaveController(this);
        evtCtrl = new GameEventController(this);

        mapCtrl = new GameMapController(this);
        characterCtrl = new GameCharacterController(this);
        itemCtrl = new GameItemController(this);
        effectCtrl = new GameEffectController(this);

        Application.targetFrameRate = 100;//先锁100帧
                                          //default Assets


        GameTexAssetForm.AddData(new GameTexAssetForm.Data(-1, GlobalNameHelper.GetDefaultTexName(""), TextureHelper.transparentTexture));
        GameTexAssetForm.AddData(new GameTexAssetForm.Data(-1, GlobalNameHelper.GetDefaultStoryTexName(), TextureHelper.transparentTexture));
        GameTexAssetForm.AddData(new GameTexAssetForm.Data(-1, GlobalNameHelper.GetDefaultCharacterTexName(), TextureHelper.transparentTexture));
        GameTexAssetForm.AddData(new GameTexAssetForm.Data(-1, GlobalNameHelper.GetDefaultItemTexName(), TextureHelper.transparentTexture));
        GameTexAssetForm.AddData(new GameTexAssetForm.Data(-1, GlobalNameHelper.GetDefaultModelTexName(), new Texture2D(1,1)));


        var res=AssetManager.instance.LoadAssetsByFolder("Z_Map/", true);
        for (int i = 0; i < res.texs.Count; i++)
        {
            GameTexAssetForm.AddData(new GameTexAssetForm.Data(-1, Path.GetFileNameWithoutExtension(res.texs[i].Item1), res.texs[i].Item2));
        }
        for (int i = 0; i < res.gos.Count; i++)
        {
            GameObjectAssetForm.AddData(new GameObjectAssetForm.Data(-1 , Path.GetFileName(res.gos[i].Item1), res.gos[i].Item2));
        }

        if (SaveAndLoad.Exist(ItemDefines.SAVE_NAME))
        {
            var lst = ItemForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(ItemDefines.SAVE_NAME)));
            for (int i = 0; i < lst.Count; i++)
            {
                ItemForm.DataById[lst[i].id].count = lst[i].count;
            }
        }

        saveCtrl.LoadOverview();

        RegisterInputDefault();
    }

    public void Start()
    {
        UiManager.instance.ShowUi<UiEnterMainCtrl>();
    }
    public void Update()
    {
    }
    public void RegisterInputDefault()
    {
        var ins = InputManager.instance;
        /*config.onButonW = () =>
        {
            var main = MapManager.instance.unitDic[4].ins.transform;
            main.position += Time.deltaTime * Vector3.forward * 2;
        }; 
        config.onButonS = () =>
        {
            var main = MapManager.instance.unitDic[4].ins.transform;
            main.position += Time.deltaTime * Vector3.back * 2;
        };
        
        config.onButonA = () =>
        {
            var main = MapManager.instance.unitDic[4].ins.transform;
            main.position += Time.deltaTime * Vector3.left * 2;
        }; 
        config.onButonD = () =>
        {
            var main = MapManager.instance.unitDic[4].ins.transform;
            main.position += Time.deltaTime * Vector3.right * 2;
        };*/


    }


   
}
