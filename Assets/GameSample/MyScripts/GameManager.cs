using Form;
using Item;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Ui;
using UnityEngine;
using Z_ByteSerialize;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Ui;
using Z_UnitSystem;

public class GameManager : Z_MonoManager<GameManager>
{
    public Vector2 downPos;
    public float dragDis2 => InputManager.instance.screenSize.x/25;
    public void Start()
    {
        RegisterInputDefault();
        UiManager.instance.ShowUi<UiEnterMainCtrl>();

        if (SaveAndLoad.Exist(ItemDefines.SAVE_NAME))
        {
            var lst=ItemForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load(ItemDefines.SAVE_NAME)));
            for(int i=0;i<lst.Count;i++)
            {
                ItemForm.DataById[lst[i].id].count = lst[i].count;
            }
        }

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
                ModSceneManager.instance.OnMouse(false, pos, dir);
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
                ModSceneManager.instance.OnMouse(true, pos, Vector3.zero);
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
