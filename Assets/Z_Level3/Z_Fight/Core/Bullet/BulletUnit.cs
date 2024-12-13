using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_UnitSystem;

namespace Z_Fight
{
    public class BulletUnit : Unit
    {
        public BulletUnit(JObject jo) : base(jo)
        {
            LoadJsonData(jo);
        }
        public BulletUnit(int uid, int prefabId_Data, Vector3 pos, Vector3 euler, Vector3 scale, int weaponBulletId_Data, float rangeLast,int attackerUid,bool selfHurt,UpdateType updateType= UpdateType.ShowOnly) : base(uid, prefabId_Data, pos, euler, scale, updateType)
        {
            this.weaponBulletId_Data = weaponBulletId_Data;
            this.rangeLast = rangeLast;
            this.attackerUid = attackerUid;
            this.selfHurt = selfHurt;
        }
        public int weaponBulletId_Data;
        public WeaponBullet weaponBullet=>FightManager.instance.data.weaponBullets[weaponBulletId_Data];
        public Vector3 dir => Quaternion.Euler(euler) * Vector3.forward;

        public float rangeLast;
        public int attackerUid;
        public bool selfHurt;

        public override Type GetInsType()
        {
            return typeof(BulletInstance);
        }

        public override void UpdateInfo()
        {
            float dis = weaponBullet.speed * Time.deltaTime;
            if (Physics.Raycast(pos, dir, out var hit))
            {
                if (hit.distance < dis * 1.5f)
                {
                    if(TryBurst(hit.transform))
                    return;
                }
            }
            pos = (pos + dis * dir);
            rangeLast -= dis;
            if (updateType == UpdateType.Always || isShowing)
            {
                ins.transform.position = pos;
            }
            if (rangeLast <= 0)
            {
                TryBurst(null);
                return;
            }


            if (isShowing)
            {
                pos = ins.transform.position;
                euler = ins.transform.eulerAngles;
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
                    if (ins != null && !(ins is BulletInstance) && (attackerUid != ins.unit.uid))
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
            FightManager.instance.RemoveUnit(uid);
        }
        private void LoadJsonData(JObject jo)
        {
  
            weaponBulletId_Data = jo.Get<int>("weaponBulletId_Data");
            rangeLast = jo.Get<int>("rangeLast");
            attackerUid = jo.Get<int>("attackerUid");
            selfHurt = jo.Get<bool>("selfHurt");

        }
        public override JObject GetJsonData()
        {
            JObject jo = base.GetJsonData();
            jo.Set("weaponBulletId_Data", weaponBulletId_Data);
            jo.Set("rangeLast", rangeLast);
            jo.Set("attackerUid", attackerUid);
            jo.Set("selfHurt", selfHurt);
            return jo;
        }
    }
}