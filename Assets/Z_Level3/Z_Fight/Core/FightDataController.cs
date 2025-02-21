using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_Fight.Form;
using Z_DesignStyle;
using Z_UnitSystem.Form;

namespace Z_Fight
{
    public class FightDataController:Z_Controller<FightManager>
    {
        public FightMainForm.Data mainData;
      
        public FightDataController(string formData)
        {
            
            WeaponBulletForm.Clear();
            FightUnitForm.Clear();
            WeaponUnitForm.Clear();
            BulletUnitForm.Clear();

            mainData = FightMainForm.GetDataByJo(JObject.Parse(formData));

            var weaponBulletDatas = WeaponBulletForm.GetDatasByJa(JArray.Parse(mainData.weaponBulletJa));
            for (int i = 0; i < weaponBulletDatas.Count; i++)
            {
                WeaponBulletForm.AddData(weaponBulletDatas[i]);
            }

            var fightDatas = FightUnitForm.GetDatasByJa(JArray.Parse(mainData.fightJa));
            for (int i = 0; i < fightDatas.Count; i++)
            {
                FightUnitForm.AddData(fightDatas[i]);
            }

            var weaponDatas = WeaponUnitForm.GetDatasByJa(JArray.Parse(mainData.weaponJa));
            for (int i = 0; i < fightDatas.Count; i++)
            {
                WeaponUnitForm.AddData(weaponDatas[i]);
            }

            var bulletDatas = BulletUnitForm.GetDatasByJa(JArray.Parse(mainData.bulletJa));
            for (int i = 0; i < bulletDatas.Count; i++)
            {
                BulletUnitForm.AddData(bulletDatas[i]);
            }
            
        }
        
        public JObject GetJsonData()
        {
            mainData.weaponBulletJa= WeaponBulletForm.GetJaByDatas().ToString();
            mainData.fightJa = FightUnitForm.GetJaByDatas().ToString();
            mainData.weaponJa = WeaponUnitForm.GetJaByDatas().ToString();
            mainData.bulletJa = BulletUnitForm.GetJaByDatas().ToString();
            return FightMainForm.GetJoByData(mainData);
        }
        public FightDataController()
        {

            WeaponBulletForm.Clear();
            FightUnitForm.Clear();
            WeaponUnitForm.Clear();
            BulletUnitForm.Clear();

            Vector3[] poss = new Vector3[] { new Vector3(0, 0, 0), new Vector3(5, 0, 5) };
            int uidCnt = 0;
            
            for (int i = 1; i < 2; i++)
            {
                WeaponBulletForm.AddData(new WeaponBulletForm.Data(
                    id:i,
                    itemId:1,
                    damage:10,
                    prefabName:"bullet1",
                    magazineCapacity:30,
                    cdTime:3,
                    reloadTime:10,
                    speed:1,
                    range:5,
                    attackPos:Vector3.zero,
                    attackDir:Vector3.zero,
                    selfHurt:false, 
                    accuracy:0.95f,
                    bulletsPer:1  
                    ));
            }

            for (int i = 0; i < poss.Length; i++)
            {
                FightUnitForm.AddData(new FightUnitForm.Data(
                     uid:++uidCnt,
                     name:"",
                     itemIdCountDic:new Dictionary<int, int> { { 1, 100 } },
                     curUsingWeaponsSid: new List<int>() { 0},
                     curReloadWeaponsSid:new List<int>(),
                     alertDistance:5,
                     hp:100,
                     hpMax:100,
                     defence:1,
                     targetFightUid:0,
                     reloadTime:0,
                     isMine:i == 0,
                     prefabName: "fight",
                     pos:poss[i],
                     euler:new Vector3(0, 0, 0),
                     scale:Vector3.one,
                     updateType: 0
                     ));
            }


            for (int i = 0; i < poss.Length; i++)
            {
                var data = new WeaponUnitForm.Data(
                     uid: ++uidCnt,
                     name:"",
                     fightUid: i + 1,
                     weaponBulletsId: new List<int>() { 1 },
                     curWeaponBulletAid: 0,
                     cdRemain: 0,
                     magazineRemain: 0,
                     prefabName: "weapon1",
                     pos: poss[i],
                     euler: new Vector3(0, 0, 0),
                     scale: Vector3.one,
                     updateType: 0
                     );
                WeaponUnitForm.AddData(data);
            }

            mainData = new FightMainForm.Data(1, uidCnt,
                FightUnitForm.GetJaByDatas().ToString(),
                WeaponUnitForm.GetJaByDatas().ToString(),
                BulletUnitForm.GetJaByDatas().ToString(),
                WeaponBulletForm.GetJaByDatas().ToString()
            );
            Debug.Log("数据：" + GetJsonData());
        }
        
        public void Unload()
        {
           foreach(var data in UnitForm.DataByUid.Values)
            {
                data.unit.Hide();
            }
        }
    }
}
