using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Fight;
using Z_Fight.Form;
using Z_UnitSystem;

public class Z_Fight_Sample : MonoBehaviour
{

    public Transform main;
    void Start()
    {
        FightManager.instance.Begin(new FightData());


        FightManager.instance.onDead += (u) =>
        {
            Debug.Log(u.ins.transform.name + "死了");
        };
        FightManager.instance.onHurt += (bullet, u) =>
        {
            Debug.Log(u.ins.transform.name + " 受击 hp:" + u.data.hp + "/" + u.data.hpMax);
        };
        FightManager.instance.onShootBullet += (bullet) =>
        {
            bullet.Show();
        };
        FightManager.instance.ShowAll();

    }

    public void Update()
    {
        FightManager.instance.UpdateInfo(UpdateSuperType.NoSuperOnly);

        FightUnitForm.DataByUid[2].unit.SetTarget(1);
        main = FightUnitForm.DataByUid[1].unit.ins.transform;
        if (Input.GetKey(KeyCode.W))
            main.position += Time.deltaTime * Vector3.forward * 2;
        if (Input.GetKey(KeyCode.S))
            main.position += Time.deltaTime * Vector3.back * 2;
        if (Input.GetKey(KeyCode.A))
            main.position += Time.deltaTime * Vector3.left * 2;
        if (Input.GetKey(KeyCode.D))
            main.position += Time.deltaTime * Vector3.right * 2;

        /*if (Input.GetKey(KeyCode.O))
            SaveAndLoad.Save("fight_data1", JsonConvert.SerializeObject(FightManager.instance.dataCtrl.GetJsonData()));
        if (Input.GetKey(KeyCode.P))
        {
            JObject jo = JObject.Parse(SaveAndLoad.Load("fight_data1"));
            FightManager.instance.Begin(new FightData(jo, prefabs, materials));
            FightManager.instance.ShowAll();
        }*/
        //MapManager.instance.AddItemUnit(new ItemUnit(8, 1, main.position + Vector3.forward, Vector3.zero, Vector3.one,true));
    }
}
