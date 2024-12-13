using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_UnitSystem;

namespace Z_Fight
{
    public class FightUnit : Unit
    {
        public FightUnit(JObject jo) : base(jo)
        {
            LoadJsonData(jo);
        }

        public FightUnit(int uid, int prefabId_Data, Vector3 pos, Vector3 eular, Vector3 scale, int targetFightUid, bool isMine, float alertDistance, Dictionary<int, int> itemCountDic, List<int> curUsingWeaponsId_subUnits, List<int> curReloadWeaponsId_subUnits, float hp, float hpMax, float defence,float reloadTime ,UpdateType updateType = UpdateType.ShowOnly) : base(uid, prefabId_Data, pos, eular, scale, updateType)
        {
            this.isMine = isMine;
            this.itemCountDic = itemCountDic;
            this.curUsingWeaponsId_subUnits = curUsingWeaponsId_subUnits;
            this.curReloadWeaponsId_subUnits = curReloadWeaponsId_subUnits;
            this.alertDistance = alertDistance;
            this.hp = hp;
            this.hpMax = hpMax;
            this.defence = defence;
            this.targetFightUid = targetFightUid;
            this.reloadTime = reloadTime;
        }
        public float hp;
        public float hpMax;
        public float defence;
        public float alertDistance;

        public bool isMine;
        public float reloadTime;
        public int targetFightUid;
        public FightUnit target => targetFightUid>0&&FightManager.instance.unitDic.ContainsKey(targetFightUid)?
            (FightUnit) FightManager.instance.unitDic[targetFightUid]:null; 
       



        public Dictionary<int, int> itemCountDic = new Dictionary<int, int>();
        public List<int> curUsingWeaponsId_subUnits = new List<int>();
        public List<int> curReloadWeaponsId_subUnits = new List<int>();
        public override Type GetInsType()
        {
            return typeof(FightInstance);
        }

        #region extern
        public void SetHp(float cur)
        {
            
            if(hp>0 && cur <= 0)
            {
                FightManager.instance.Dead(this);
            }
            if (cur > hpMax) cur = hpMax;
            if (cur < 0) cur = 0;
            hp = cur;
        }
        public int TryGetItem(int itemId)
        {
            if (itemCountDic.ContainsKey(itemId))
            {
                return itemCountDic[itemId];
            }
            return 0;
        }

        public int TryGetItem(int itemId, int require)
        {
            if (itemCountDic.ContainsKey(itemId))
            {
                if (itemCountDic[itemId] < require)
                {
                    require = itemCountDic[itemId];

                }
                itemCountDic[itemId] -= require;
                return require;
            }
            return 0;
        }
        public void TryAddItem(int itemId, int require)
        {
            Debug.Log(itemId + " " + require + " " + itemCountDic[itemId]);
            if (!itemCountDic.ContainsKey(itemId))
                itemCountDic[itemId] = 0;
            itemCountDic[itemId] += require;
        }
        public void SetTarget(int tar)
        {
            targetFightUid = tar;
        }

        #endregion
        public List<int> GetCanAttackIds_subUnits()
        {
            
            List<int> weapons = new List<int>();
            if (target != null)
                foreach (var id in curUsingWeaponsId_subUnits)
            {
                var unit = subUnits[id];
                if (unit is WeaponUnit weapon)
                {
                    var dir = (target.pos - weapon.pos);
                    if (dir.sqrMagnitude < alertDistance * alertDistance)
                    {
                            //Debug.Log(weapon.weaponBullet(weapon.curBulletId_Data).range);
                            //扫默认层
                        if (Physics.Raycast(pos, target.pos - pos, out var hit, weapon.weaponBullet(weapon.curBulletId_Data).range,1))
                        {
                                if (hit.rigidbody == target.ins.rigidbody)
                            {
                                weapons.Add(id);
                            }
                        }
                    }
                }
            }
            return weapons;
        }
        //1.有能打的先打
        //2.没能打的装填能打的
        public override void UpdateInfo()
        {
            
            var attackWeaponIds_subUnits = GetCanAttackIds_subUnits();

            //调整姿态
            if (attackWeaponIds_subUnits.Count > 0)
            {
                var dir = (target.pos - subUnits[attackWeaponIds_subUnits[0]].pos);
                Quaternion rotation = Quaternion.LookRotation(dir);
                euler = rotation.eulerAngles;
            }


            base.UpdateInfo();


            bool reloading = true;
            for (int i = 0; i < attackWeaponIds_subUnits.Count; i++)
            {
                var weapon = subUnits[attackWeaponIds_subUnits[i]] as WeaponUnit;
                
                if (weapon.magazineRemain > 0)
                    reloading = false;
            }
            //全打不了
            if (reloading)
            {
                //1.有在装的
                if (curReloadWeaponsId_subUnits.Count > 0)
                {
                    for (int i = 0; i < curReloadWeaponsId_subUnits.Count; i++)
                    {
                        var unit = subUnits[curReloadWeaponsId_subUnits[i]];
                        if (unit is WeaponUnit weapon)
                        {

                            if (weapon.TryReload(reloadTime))
                            {
                                curReloadWeaponsId_subUnits.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
                //2.有能打没在装的
                else if (attackWeaponIds_subUnits.Count > 0)
                {
                    for (int i = 0; i < attackWeaponIds_subUnits.Count; i++)
                    {
                        var weapon = subUnits[attackWeaponIds_subUnits[i]] as WeaponUnit;
                        if (weapon.CanReload())
                        {
                            curReloadWeaponsId_subUnits.Add(attackWeaponIds_subUnits[i]);
                            break;
                        }
                    }

                }
                //3.没有能打的，全装了
                else
                {
                    foreach (var id in curUsingWeaponsId_subUnits)
                    {
                        var weapon = subUnits[id] as WeaponUnit;
                        if (weapon.CanReload())
                        {
                            curReloadWeaponsId_subUnits.Add(id);
                            break;
                        }
                    }
                }

                //没有能装的，靠近目标
                if (curReloadWeaponsId_subUnits.Count==0)
                {
                    reloadTime = 0;
                    //
                }
                else
                //装填中
                {
                    reloadTime += Time.deltaTime;
                }
            }
            else
            {
                reloadTime = 0;
                curReloadWeaponsId_subUnits.Clear();
                for (int i = 0; i < attackWeaponIds_subUnits.Count; i++)
                {
                    var weapon=subUnits[attackWeaponIds_subUnits[i]] as WeaponUnit;
                    weapon.TryShoot();
                }
            }
            if (isShowing)
            {
                pos = ins.transform.position;
                euler = ins.transform.eulerAngles;
            }

        }
        private void LoadJsonData(JObject jo)
        {
            isMine = jo.Get<bool>("isMine");
            itemCountDic = jo.Get<Dictionary<int,int>>("itemCountDic");
            curUsingWeaponsId_subUnits = jo.Get <List<int>>("curUsingWeaponsId_subUnits");
            curReloadWeaponsId_subUnits = jo.Get<List<int>>("curReloadWeaponsId_subUnits");
            alertDistance = jo.Get<float>("alertDistance");
            hp = jo.Get<float>("hp");
            hpMax = jo.Get<float>("hpMax");
            defence = jo.Get<float>("defence");
            targetFightUid = jo.Get<int>("targetFightUid");
            reloadTime = jo.Get<int>("reloadTime");
        }
        public override JObject GetJsonData()
        {
            JObject jo = base.GetJsonData();
            jo.Set("isMine", isMine);
            jo.Set("itemCountDic", itemCountDic);
            jo.Set("curUsingWeaponsId_subUnits", curUsingWeaponsId_subUnits);
            jo.Set("curReloadWeaponsId_subUnits", curReloadWeaponsId_subUnits);
            jo.Set("alertDistance", alertDistance);
            jo.Set("hp", hp);
            jo.Set("hpMax", hpMax);
            jo.Set("defence", defence);
            jo.Set("targetFightUid", targetFightUid); 
            jo.Set("reloadTime", reloadTime); 

            return jo;
        }
    }
}

