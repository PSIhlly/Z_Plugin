using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_UnitSystem;

namespace Z_Fight
{



    public class WeaponUnit : Unit
    {
        public WeaponUnit(JObject jo) : base(jo)
        {
            LoadJsonData(jo);
        }
        public WeaponUnit(int uid, int fightUid_Data, int prefabId_Data, Vector3 pos, Vector3 eular, Vector3 scale,
            int curBulletId_Data,List<int> weaponBulletIds_Data, float cdTime, float cdRemain, float reloadTime,int magazineRemain,
                UpdateType updateType = UpdateType.ShowOnly) : base(uid, prefabId_Data, pos, eular, scale, updateType)
        {
            this.fightUid_Data = fightUid_Data;
            this.weaponBulletIds_Data = weaponBulletIds_Data;
            this.curBulletId_Data = curBulletId_Data;
            this.cdTime = cdTime;
            this.cdRemain = cdRemain;
            this.reloadTime = reloadTime;
            this.magazineRemain = magazineRemain;
        }
        public List<int> bullet = new List<int>();

        public int curBulletId_Data;

        public WeaponBullet weaponBullet(int weaponBulletId_Data) =>FightManager.instance.data.weaponBullets[weaponBulletId_Data];
   
        public List<int> weaponBulletIds_Data;

        public float cdTime;
        public float cdRemain;

        public float reloadTime;

        public int magazineRemain;

        public int fightUid_Data;


        public override Type GetInsType()
        {
            return typeof(WeaponInstance);
        }
        public void TryShoot()
        {
       
            if (magazineRemain>0 && cdRemain<=0)
            {
                var cur = weaponBullet(curBulletId_Data);
                for(int i=0;i<cur.bulletsPer;i++)
                {
                    var dir = Quaternion.Euler(euler + cur.attackDir) * Vector3.forward;
                    var err=Z_Math.Graph.GetRandomVectorOnPlane(dir, (1-cur.accuracy)*dir.magnitude);
                    dir=dir + err;
                    var bullet = new BulletUnit(++FightManager.instance.data.uidCnt, cur.prefabId_Data, pos + cur.attackPos,Quaternion.LookRotation(dir).eulerAngles , Vector3.one, curBulletId_Data, cur.range, superUnit.uid, cur.selfHurt);
                    FightManager.instance.AddUnit(bullet);
                    bullet.Show();
                }

                cdRemain = cdTime;
                magazineRemain--;

            }
        }
        public bool TryReload(float time)
        {
            if (time > reloadTime)
            {
                FightUnit fight = (FightUnit)superUnit;
                fight.TryAddItem(weaponBullet(curBulletId_Data).itemId_Data, magazineRemain);
                magazineRemain = fight.TryGetItem(weaponBullet(curBulletId_Data).itemId_Data, weaponBullet(curBulletId_Data).magazineCapacity);

                return true;
            }
            return false;
        }
        public bool CanReload()
        {
            if (weaponBullet(curBulletId_Data).magazineCapacity == magazineRemain)
                return false;
            FightUnit fight = (FightUnit)superUnit;
            return fight.TryGetItem(curBulletId_Data)>0;
        }

        public override void UpdateInfo()
        {
            if ((updateType == UpdateType.Always || isShowing))
            {
                //持握姿势影响
                pos = superUnit.pos;
                euler = superUnit.euler;

            }
            if (cdRemain > 0)
                cdRemain -= Time.deltaTime;



            if (isShowing)
            {
                ins.transform.position = pos;
                ins.transform.eulerAngles = euler;
            }
            base.UpdateInfo();
        }
        private void LoadJsonData(JObject jo)
        {
            fightUid_Data = jo.Get<int>("fightUid_Data");
            weaponBulletIds_Data = jo.Get<List<int>>("weaponBulletIds_Data");
            curBulletId_Data = jo.Get<int>("curBulletId_Data");
            cdTime = jo.Get<float>("cdTime");
            cdRemain = jo.Get<float>("cdRemain");
            reloadTime = jo.Get<float>("reloadTime");
            magazineRemain = jo.Get<int>("magazineRemain");
          
        }
        public override JObject GetJsonData()
        {
            JObject jo = base.GetJsonData();
            jo.Set("fightUid_Data", fightUid_Data);
            jo.Set("weaponBulletIds_Data", weaponBulletIds_Data);
            jo.Set("curBulletId_Data", curBulletId_Data);
            jo.Set("cdTime", cdTime);
            jo.Set("cdRemain", cdRemain);
            jo.Set("reloadTime", reloadTime);
            jo.Set("magazineRemain", magazineRemain);
            return jo;
        }
    }
}