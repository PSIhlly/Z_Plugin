using Form;
using Item;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Ui;
using Ui.EnterMain;
using UnityEngine;
using UnityEngine.Rendering;
using Z_ByteSerialize;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Ui;
using Z_UnitSystem;
public static class GlobalSettings
{
    public static int CHARACTER_AVATA_MAX => Character.GlobalSettings.CHARACTER_AVATA_MAX;
    public static int CHARACTER_ANIM_MAX => Character.GlobalSettings.CHARACTER_ANIM_MAX;
    public static int TEX_ANIM_MAX =>  Z_Map.GlobalSettings.TEX_ANIM_MAX;
    public static int ITEM_UNIT_MAX =>  Z_Map.GlobalSettings.ITEM_UNIT_MAX;
    public static bool NAV_DEBUG =>  Z_Map.GlobalSettings.NAV_DEBUG;
    public static bool MAP_SHOW_DEBUG =>  Z_Map.GlobalSettings.MAP_SHOW_DEBUG;
    public static bool OVERLAY_HIDE =>  Z_Map.GlobalSettings.OVERLAY_HIDE;
}
    public static class GlobalHelper
{
    public static string GetCharacterAvatarName(string nickName, int id) => Character.GlobalHelper.GetCharacterAvatarName(nickName, id);
    public static string GetCharacterAnimName(string nickName,string animName,int part, int id) => Character.GlobalHelper.GetCharacterAnimName(nickName, animName, part,id);
    public static string GetTexRealName(string nickName, int animId) => Z_Map.GlobalHelper.GetTexRealName(nickName, animId);
    public static string GetTexNickName(string realName) => Z_Map.GlobalHelper.GetTexNickName(realName);
    public static string GetMaskRealName(string nickName, int maskId) => Z_Map.GlobalHelper.GetMaskRealName(nickName,maskId);
    public static string GetInternalPrefabName(string name) => Z_Map.GlobalHelper.GetInternalPrefabName(name);
    public static string GetItemTexRealName(string nickName, int prefabId) => Z_Map.GlobalHelper.GetItemTexRealName(nickName, prefabId);
}

public class GameManager : Z_MonoManager<GameManager>
{
    public GameUtilController utilCtrl;
    public Vector2 downPos;
    public Material mat;
    public Mesh mesh;
    public float dragDis2 => InputManager.instance.screenSize.x/25;

    public override void Init()
    {
        base.Init();


        utilCtrl = new GameUtilController(this);

        Application.targetFrameRate = 100;//先锁100帧
        //default Assets
        AssetManager.instance.LoadAssetsByFolderAutoAdd("Z_Map/", true, true);


        if (SaveAndLoad.Exist(ItemDefines.SAVE_NAME))
        {
            var lst = ItemForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load(ItemDefines.SAVE_NAME)));
            for (int i = 0; i < lst.Count; i++)
            {
                ItemForm.DataById[lst[i].id].count = lst[i].count;
            }
        }


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
        var config = new InputConfig();

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

        config.onButtonDownE = () =>
        {
            Cmd.instance.Excute("AddItem 1 5");
        };
        ins.Register(config);
    }
    public void RegisterInputByUgc()
    {
        var ins = InputManager.instance;
        var config = new InputConfig();

        config.onButtonW = () =>
        {
            CameraInstance.instance.tarTrs.position += Time.deltaTime * Vector3.forward * 4;
        };
        config.onButtonS = () =>
        {
            CameraInstance.instance.tarTrs.position += Time.deltaTime * Vector3.back * 4;
        };

        config.onButtonA = () =>
        {
            CameraInstance.instance.tarTrs.position += Time.deltaTime * Vector3.left * 4;
        };
        config.onButtonD = () =>
        {
            CameraInstance.instance.tarTrs.position += Time.deltaTime * Vector3.right * 4;
        };
        config.onMouse = (id, pos, dir,ui) =>
        {
            
            if (id==0&&ui==null&& downPos != Vector2.zero )//&& (downPos - new Vector2(pos.x, pos.y)).sqrMagnitude > dragDis2
            {
                ModManager.instance.OnMouse(false, pos, dir);
            }
        };

        config.onMouseDown = (id, pos, ui) =>
        {
            
            if (ui==null)
            {
                downPos = pos;
            }
            else
            {
                downPos = Vector2.zero;
            }
        };
        config.onMouseUp = (id, pos, ui) =>
        {
            if (id == 0 && ui == null && downPos != Vector2.zero && (downPos - new Vector2(pos.x, pos.y)).sqrMagnitude < dragDis2)
            {
                ModManager.instance.OnMouse(true, pos, Vector3.zero);
            }
            downPos = Vector2.zero;
        };

        config.onMouseScroll = (v) =>
        {
            CameraInstance.instance.cam.orthographicSize += v*-2f;
        };

        ins.Register(config);
    }
}
