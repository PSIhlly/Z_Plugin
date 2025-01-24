using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_Debug;
using Z_Fight.Form;
using Z_UnitSystem;

namespace Z_Fight
{
    public class WeaponUnit : Unit
    {
        public WeaponUnit(WeaponUnitForm.Data data):base(data)
        {
        }
        public WeaponUnitForm.Data data=>(WeaponUnitForm.Data)_data;

        public WeaponBulletForm.Data weaponBullet(int weaponBulletAid) => WeaponBulletForm.DataById[data.weaponBulletsId[weaponBulletAid]];
   
        public override Type GetInsType()
        {
            return typeof(WeaponInstance);
        }
        public void TryShoot()
        {
            if (GlobalSettings.WEAPON_SHOOT_DEBUG)
            {
                Z_Log.Log(data.uid +" magazine:"+ data.magazineRemain+ " cd:"+ data.cdRemain);
            }
            if (data.magazineRemain>0 && data.cdRemain <=0)
            {
                var cur = weaponBullet(data.curWeaponBulletAid);
                for(int i=0;i<cur.bulletsPer;i++)
                {
                    var dir = Quaternion.Euler(data.euler + cur.attackDir) * Vector3.forward;
                    var err=Z_Math.Graph.GetRandomVectorOnPlane(dir, (1-cur.accuracy)*dir.magnitude);
                    dir=dir + err;

                    var bulletData = new BulletUnitForm.Data(
                        uid: ++FightManager.instance.dataCtrl.mainData.uidCnt,
                        weaponBulletId: cur.id,
                        rangeLast: cur.range,
                        attackerUid: superUnit.data.uid,
                        prefabName: cur.prefabName,
                        pos: data.pos + cur.attackPos,
                        euler: Quaternion.LookRotation(dir).eulerAngles,
                        scale: Vector3.one,
                        updateType: 0
                    );
                    FightManager.instance.AddUnit(bulletData.unit);
                }

                data.cdRemain = cur.cdTime;
                data.magazineRemain--;

            }
        }
        public bool TryReload(float time)
        {
            var cur = weaponBullet(data.curWeaponBulletAid);
            if (GlobalSettings.WEAPON_LOAD_DEBUG)
            {
                Z_Log.Log(data.uid + "loading:"+ time+" > " + cur.reloadTime);
            }
            if (time > cur.reloadTime)
            {
                FightUnit fight = (FightUnit)superUnit;
                fight.TryAddItem(cur.itemId, data.magazineRemain);
                data.magazineRemain = fight.TryGetItem(cur.itemId, cur.magazineCapacity);
                if (GlobalSettings.WEAPON_LOAD_DEBUG)
                {
                    Z_Log.Log(data.uid + "loaded:" + data.magazineRemain+" has:"+ fight.TryGetItem(cur.itemId, cur.magazineCapacity));
                }
                return true;
            }
            return false;
        }
        public bool CanReload()
        {
            if (weaponBullet(data.curWeaponBulletAid).magazineCapacity == data.magazineRemain)
                return false;
            FightUnit fight = (FightUnit)superUnit;
            return fight.TryGetItem(weaponBullet(data.curWeaponBulletAid).itemId) >0;
        }

        public override void UpdateInfo()
        {
            if ((data.updateType == (int)UpdateType.Always || isShowing))
            {
                //持握姿势影响
                data.pos = superUnit.data.pos;
                data.euler = superUnit.data.euler;
            }
            if (data.cdRemain > 0)
                data.cdRemain -= Time.deltaTime;



            if (isShowing)
            {
                ins.transform.position = data.pos;
                ins.transform.eulerAngles = data.euler;
            }
            base.UpdateInfo();
        }
        
    }
}