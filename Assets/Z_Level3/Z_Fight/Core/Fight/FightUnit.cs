using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_Fight.Form;
using Z_UnitSystem;
using Z_Debug;
namespace Z_Fight
{
    public partial class FightUnit : Unit
    {
        public FightUnit(FightUnitForm.Data data) : base(data)
        {
        }
        public FightUnitForm.Data data => (FightUnitForm.Data)_data;

        public FightInstance ins
        {
            set { base.ins = value; }
            get { return (FightInstance)base.ins; }
        }
        public FightUnit target => data.targetFightUid >0&&FightUnitForm.DataByUid.ContainsKey(data.targetFightUid)?
            (FightUnit) FightUnitForm.DataByUid[data.targetFightUid].unit:null; 
       
        public override Type GetInsType()
        {
            return typeof(FightInstance);
        }

        #region extern
        public void SetHp(float cur)
        {
            
            if(data.hp >0 && cur <= 0)
            {
                FightManager.instance.Dead(this);
            }
            if (cur > data.hpMax) cur = data.hpMax;
            if (cur < 0) cur = 0;
            data.hp = cur;
        }
        public int TryGetItem(int itemId)
        {
            if (data.itemIdCountDic.ContainsKey(itemId))
            {
                return data.itemIdCountDic[itemId];
            }
            return 0;
        }

        public int TryGetItem(int itemId, int require)
        {
            if (data.itemIdCountDic.ContainsKey(itemId))
            {
                if (data.itemIdCountDic[itemId] < require)
                {
                    require = data.itemIdCountDic[itemId];

                }
                data.itemIdCountDic[itemId] -= require;
                return require;
            }
            return 0;
        }
        public void TryAddItem(int itemId, int require)
        {
            if (!data.itemIdCountDic.ContainsKey(itemId))
                data.itemIdCountDic[itemId] = 0;
            data.itemIdCountDic[itemId] += require;
        }
        public void SetTarget(int tar)
        {
            data.targetFightUid = tar;
        }

        #endregion
        public List<int> GetCanAttackIds_subUnits()
        {
            
            List<int> weapons = new List<int>();
            if (target != null)
                foreach (var id in data.curUsingWeaponsSid)
            {
                    var unit = subUnits[id];
                if (unit is WeaponUnit weapon)
                {

                        var dir = (target.data.pos - weapon.data.pos);

                        if (GlobalSettings.FIGHT_FIND_DEBUG)
                        {
                            Z_Log.Log(data.uid + " cur weaponAid: " + id+"  length:"+ dir.sqrMagnitude+" alert:"+ data.alertDistance+" range:"+ weapon.weaponBullet(weapon.data.curWeaponBulletAid).range);
                        }
                        if (dir.sqrMagnitude < data.alertDistance * data.alertDistance)
                    {
                            if (GlobalSettings.FIGHT_FIND_DEBUG)
                            {
                                Z_Log.Log(data.pos+" -> "+target.data.pos);
                                Debug.DrawLine(data.pos, data.pos + (target.data.pos - data.pos).normalized* weapon.weaponBullet(weapon.data.curWeaponBulletAid).range);
                            }
                            //扫默认层
                            if (Physics.Raycast(data.pos, target.data.pos - data.pos, out var hit, weapon.weaponBullet(weapon.data.curWeaponBulletAid).range,1))
                        {
                                if (GlobalSettings.FIGHT_FIND_DEBUG)
                                {
                                    Z_Log.Log(data.uid + " hit " + hit.transform.name);
                                }
                                if (hit.rigidbody == target.ins.rigidbody)
                            {
                                if (GlobalSettings.FIGHT_FIND_DEBUG)
                                {
                                   Z_Log.Log(data.uid+" found "+ target.data.uid+" weaponAid:"+id);
                                }
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
                var dir = (target.data.pos - subUnits[attackWeaponIds_subUnits[0]].data.pos);
                Quaternion rotation = Quaternion.LookRotation(dir);
                data.euler = rotation.eulerAngles;
            }


            base.UpdateInfo();


            bool reloading = true;
            for (int i = 0; i < attackWeaponIds_subUnits.Count; i++)
            {
                var weapon = subUnits[attackWeaponIds_subUnits[i]] as WeaponUnit;

                if (weapon.data.magazineRemain > 0)
                {
                    
                    reloading = false; 
                }
            }
            
            //全打不了
            if (reloading)
            {
                if (GlobalSettings.FIGHT_SHOOT_DEBUG)
                {
                    Z_Log.Log(data.uid + " no magazine ");
                }
                //1.有在装的
                if (data.curReloadWeaponsSid.Count > 0)
                {
                    for (int i = 0; i < data.curReloadWeaponsSid.Count; i++)
                    {
                        var unit = subUnits[data.curReloadWeaponsSid[i]];
                        if (unit is WeaponUnit weapon)
                        {

                            if (weapon.TryReload(data.reloadTime))
                            {
                                data.curReloadWeaponsSid.RemoveAt(i);
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
                            data.curReloadWeaponsSid.Add(attackWeaponIds_subUnits[i]);
                            break;
                        }
                    }

                }
                //3.没有能打的，全装了
                else
                {
                    foreach (var id in data.curUsingWeaponsSid)
                    {
                        var weapon = subUnits[id] as WeaponUnit;
                        if (weapon.CanReload())
                        {
                            data.curReloadWeaponsSid.Add(id);
                            break;
                        }
                    }
                }

                //没有能装的，靠近目标
                if (data.curReloadWeaponsSid.Count==0)
                {
                    data.reloadTime = 0;
                    //
                }
                else
                //装填中
                {
                    data.reloadTime += Time.deltaTime;
                }
            }
            else
            {
                data.reloadTime = 0;
                data.curReloadWeaponsSid.Clear();
                for (int i = 0; i < attackWeaponIds_subUnits.Count; i++)
                {
                    var weapon=subUnits[attackWeaponIds_subUnits[i]] as WeaponUnit;
                    if (GlobalSettings.FIGHT_SHOOT_DEBUG)
                    {
                        Z_Log.Log(data.uid + " try shoot ");
                    }
                    weapon.TryShoot();
                }
            }
            if (isShowing)
            {
                data.pos = ins.transform.position;
                data.euler = ins.transform.eulerAngles;
            }

        }
    
 
      
    }
}

