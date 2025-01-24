using Form;
using Item;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Ui;
using UnityEngine;
using Z_Input;
using Z_Map;
using Z_Ui;
using Z_UnitSystem;

public class GameManager : MonoBehaviour
{
    public void Start()
    {
        RegisterInput();
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
    public void RegisterInput()
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

        config.onButonDownE = () =>
        {
            Cmd.instance.Excute("AddItem 1 5");
        };
        ins.Register(config);
    }
}
