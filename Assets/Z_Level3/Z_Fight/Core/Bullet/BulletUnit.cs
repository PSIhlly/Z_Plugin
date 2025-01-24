using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_Fight.Form;
using Z_UnitSystem;

namespace Z_Fight
{
    public class BulletUnit : Unit
    {
        public BulletUnit(BulletUnitForm.Data data) : base(data)
        {
        }
        public BulletUnitForm.Data data => (BulletUnitForm.Data)_data;

        public WeaponBulletForm.Data weaponBullet=> WeaponBulletForm.DataById[data.weaponBulletId];
        public Vector3 dir => Quaternion.Euler(data.euler) * Vector3.forward;


        public override Type GetInsType()
        {
            return typeof(BulletInstance);
        }

        public override void UpdateInfo()
        {
            float dis = weaponBullet.speed * Time.deltaTime;
            if (Physics.Raycast(data.pos, dir, out var hit))
            {
                if (hit.distance < dis * 1.5f)
                {
                    if(TryBurst(hit.transform))
                    return;
                }
            }
            data.pos = (data.pos + dis * dir);
            data.rangeLast -= dis;
            if (data.updateType == (int)UpdateType.Always || isShowing)
            {
                ins.transform.position = data.pos;
            }
            if (data.rangeLast <= 0)
            {
                TryBurst(null);
                return;
            }


            if (isShowing)
            {
                data.pos = ins.transform.position;
                data.euler = ins.transform.eulerAngles;
            }
        }
        public bool TryBurst(Transform tar)
        {
            if(tar!=null)
            {
                var inss = tar.GetComponents<Instance>();
                foreach(var ins in inss)
                {
                    //子弹不碰子弹
                    if (ins != null && !(ins is BulletInstance) && (data.attackerUid != ins.unit.data.uid))
                    {
                        Hurt(this, ins.unit);
                        Des();
                        return true;
                    }
                }
                return false;
            }
            Des();
            return true;
        }
        public void Hurt(BulletUnit bullet,Unit tar)
        {
            FightManager.instance.Hurt(bullet, tar);
        }
        public void Des()
        {
            FightManager.instance.RemoveUnit(data.uid);
        }
        
    }
}