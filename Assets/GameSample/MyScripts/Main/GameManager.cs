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
using TreeEditor;
using Ui;
using Ui.EnterMain;
using Ui.ModStory.ModStoryEffect.ModStoryEffectUnit;
using Ui.PlaySceneMenu;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Audio;
using Z_ByteSerialize;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_Input;
using Z_Language;
using Z_Map;
using Z_Map.Form;
using Z_Math;
using Z_Texture;
using Z_Ui;
using Z_Ui.Dialog;
using Z_UnitSystem;
using static UnityEngine.Rendering.DebugUI.Table;
namespace Form
{

    public static partial class GameParamForm
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void RegisterGet()
        {
            beforeGetAction = (data) =>
            {
                data.v = BoxDataForm.GetJoByData(data.GetValue()).ToString();
            };
        }

        public partial class Data
        {
            BoxDataForm.Data _boxValue;
            public BoxDataForm.Data GetValue()
            {
                return Get(ref _boxValue, v);
            }

            public void SetValue(object v)
            {
                Set(GetValue(), v);
            }

            private BoxDataForm.Data Get(ref BoxDataForm.Data box, string str)
            {
                if (box == null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(str))
                            box = BoxDataForm.GetDataByJo(JObject.Parse(str));
                        else
                            box = CodeHelper.CreateBox();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        box = CodeHelper.CreateBox();
                    }
                }

                return box;
            }
            private void Set(BoxDataForm.Data box, object o)
            {
                if (o is string str)
                {
                    box.str = str;
                }
                else if (o is float num)
                {
                    box.num = num;
                    box.str = null;
                }
                else if (o is int i)
                {
                    box.num = i;
                    box.str = null;
                }
                else if (o is BoxDataForm.Data b)
                {
                    box.Reset(b.DeepCopy());
                }
            }

        }
    }
}


public class CameraMoveEvent : Z_Event
{


}

public static partial class GlobalSettings
{
    public static string BGM_FILE_NAME = "Bgm.mp3";
    public static float DRAG_DIS2 => InputManager.instance.screenSize.x / 25;
    public static int TERRAIN_LAYER_MAX => 3;
    public static int TEXTURE_MAX = 100000;
    public static int TEXTURE_MASK_MAX = 100000;
    public static int OBJECT_MAX = 100000;
    public static float MAX = 999999999;
}
public static class GlobalDefaultHelper
{
    public static string defaultLab = "unclassified";
    public static string GetInternalPrefabName(string name) => MapInfo.GetPrefabName(name);

    public static string GetRuntimePrefabName(string name = "") => "runtime$" + name;
    public static string GetRuntimeMapObjectPrefabName(int mapObjectId) => GetRuntimePrefabName("obj$" + mapObjectId);
    public static string GetRuntimeMapItemPrefabName(int mapItemId) => GetRuntimePrefabName("item$" + mapItemId);
    public static int DefaultTexId;
    public static int ExternDefaultTexId;

    public static string GetDefaultVideoName(string name = "") => "reservedV$" + name;
    public static string GetExternDefaultVideoName(string name = "") => AssetManager.instance.videoCtrl.GetName();
    public static string GetDefaultAudioName(string name = "") => "reservedA$" + name;
    public static string GetExternDefaultAudioName(string name = "") => AssetManager.instance.audioCtrl.GetName();
    public static bool IsInnerAssetName(int id)
    {
        return !StoryTexAssetForm.DataById.ContainsKey(id) && !StoryAudioAssetForm.DataById.ContainsKey(id) && !StoryVideoAssetForm.DataById.ContainsKey(id);
    }

    public static int DefaultStoryTexId;
    public static int DefaultEventTexId;
    public static int DefaultCharacterTexId;
    public static int DefaultItemTexId;
    public static int DefaultModelTexId;
}




public class GameManager : Z_MonoManager<GameManager>
{
    public GameUtilController utilCtrl;
    public GameSaveController saveCtrl;
    public GameEventController evtCtrl;
    public GameMapController mapCtrl;
    public GameCharacterController characterCtrl;
    public GameItemController itemCtrl;


    public StoryForm.Data curStory;
    public SceneForm.Data curScene;
    public ProgressForm.Data curProgress => ProgressForm.DataByUid.ContainsKey(1) ? ProgressForm.DataByUid[1] : null;
    public Dictionary<string, AssetForm.Data> innerAssetDic; 
    public override void Init()
    {
        innerAssetDic = new Dictionary<string, AssetForm.Data>();
        base.Init();
        AudioManager.instance.BgmStreaming(GlobalSettings.BGM_FILE_NAME);
        LanguageManager.instance.SetLanguage(Language.Cn);

        utilCtrl = new GameUtilController(this);
        saveCtrl = new GameSaveController(this);
        evtCtrl = new GameEventController(this);

        mapCtrl = new GameMapController(this);
        characterCtrl = new GameCharacterController(this);
        itemCtrl = new GameItemController(this);



        Application.targetFrameRate = 100;//先锁100帧
                                          //default Assets
        var data = AssetManager.instance.texCtrl.CreateDataByBytes(TextureHelper.GetTextureByte(TextureHelper.transparentTexture), "");
        data.id=AssetForm.autoIdCnt + 200000;
        saveCtrl.AddStoryTex(ref data);
        GlobalDefaultHelper.DefaultTexId = data.id;
        GlobalDefaultHelper.DefaultStoryTexId = data.id;
        GlobalDefaultHelper.DefaultEventTexId = data.id;
        GlobalDefaultHelper.DefaultCharacterTexId = data.id;
        GlobalDefaultHelper.DefaultItemTexId = data.id;
        GlobalDefaultHelper.DefaultModelTexId = data.id;
        GlobalDefaultHelper.ExternDefaultTexId = data.id;



        var res = AssetManager.instance.LoadAssetsByFolder("Z_Map/", true);
        for (int i = 0; i < res.texs.Count; i++)
        {
            var texData = res.texs[i].Item2;
            texData.id = AssetForm.autoIdCnt+ Algorithm.GetHash(texData.name,9973) + 100000;
            saveCtrl.AddGameTex(ref texData);
            if(texData.name == "MapTexture$grass")
            {
                innerAssetDic["defaultTileTexture"] = texData;
            }
            if (texData.name == "MapTexture$wall")
            {
                innerAssetDic["defaultObjectTexture"] = texData;
            }
            
        }
        for (int i = 0; i < res.gos.Count; i++)
        {
            if (res.gos[i].Item1.StartsWith(MapInfo.GetPrefabName()))
            {
                res.gos[i].Item2.id = AssetForm.autoIdCnt + Algorithm.GetHash(res.gos[i].Item1, 9973) + 110000;
                innerAssetDic[res.gos[i].Item1.Split(MapInfo.GetPrefabName())[1]] = res.gos[i].Item2;
            }
            GameObjectAssetForm.AddData(res.gos[i].Item2);
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



        DialogManager.instance.SetMenuAct(() =>
        {
            UiManager.instance.ShowUi<UiPlaySceneMenuCtrl>();
        });

    }

    public void Start()
    {
        UiManager.instance.ShowUi<UiEnterMainCtrl>();
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
    private static int playerPosToMapPosOffset = 500;
    public static float PlayerPosToMapPos(float v)
    {
        return (v + playerPosToMapPosOffset);
    }
    public static float MapPosToPlayerPos(float v)
    {
        return (v - playerPosToMapPosOffset);
    }
    public static Vector3 PlayerPosToMapPos(Vector3 playerPos)
    {
        return playerPos + Vector3.one * playerPosToMapPosOffset;
    }
    public static Vector3 MapPosToPlayerPos(Vector3 mapPos)
    {
        return mapPos - Vector3.one * playerPosToMapPosOffset;
    }
}
