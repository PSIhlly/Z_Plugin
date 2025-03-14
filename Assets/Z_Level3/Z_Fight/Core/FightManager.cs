using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Debug;
using Z_DesignStyle;
using Z_Fight.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;

namespace Z_Fight
{
    public static class GlobalSettings
    {
        public const bool FIGHT_FIND_DEBUG = false;
        public const bool FIGHT_SHOOT_DEBUG = false;
        public const bool WEAPON_LOAD_DEBUG = false;
        public const bool WEAPON_SHOOT_DEBUG = false;
        public const bool MAIN_ADDUNIT_DEBUG = false;
    }
    public enum UpdateSuperType
    {
        All,
        NoSuperOnly
    }
    public class FightManager : Z_MonoManager<FightManager>
    {

        public FightData dataCtrl;
        public GameObject mainGo;

        public Action<BulletUnit, FightUnit> onHurt;
        public Action<FightUnit> onDead;
        public Action<BulletUnit> onShootBullet;

        public override void Init()
        {
            base.Init();
        }

        #region external

        public void Begin(FightData dataCtrl)
        {
            End();
            Init();

            mainGo.SetActive(true);
            this.dataCtrl = dataCtrl;
            foreach (var wpData in WeaponUnitForm.DataByUid.Values)
            {
                FightUnitForm.DataByUid[wpData.fightUid].unit.Bind(wpData.unit);
            }
        }
        
        public void AddUnit(Unit unit)
        {
            if (GlobalSettings.MAIN_ADDUNIT_DEBUG)
            {
                Z_Log.Log("Add "+unit);
            }
            unit.SubUpdateActive();
            if (unit is FightUnit fight)
            {
                FightUnitForm.AddData(fight.data);
            }
            else if (unit is WeaponUnit weapon)
            {
                WeaponUnitForm.AddData(weapon.data);
            }
            else if (unit is BulletUnit bullet)
            {
                BulletUnitForm.AddData(bullet.data);
                onShootBullet?.Invoke(bullet);
            }


        }
        public void RemoveUnit(int uid)
        {
            var data = UnitForm.DataByUid[uid];
            var unit = data.unit;
            if (data is FightUnitForm.Data)
            {
                FightUnitForm.RemoveData(uid);
            }
            if (data is WeaponUnitForm.Data)
            {
                WeaponUnitForm.RemoveData(uid);
            }
            if (data is BulletUnitForm.Data)
            {
                BulletUnitForm.RemoveData(uid);
            }
            if(unit.superUnit!=null)
                unit.superUnit.Unbind(data.unit);
            unit.VisOff();
            unit.Hide();
        }
        public void SetTarget(FightUnit self, int tar)
        {
            self.SetTarget(tar);
        }
        public void Hurt(BulletUnit bullet, Unit tar)
        {
            if(tar is FightUnit fight)
            {
                var hurt = bullet.weaponBullet.damage - fight.data.defence;
                if (hurt < 0) hurt = 0;
                fight.SetHp(fight.data.hp - hurt);
                onHurt?.Invoke(bullet, fight);
            }
        }
        public void Dead(FightUnit tar)
        {
            onDead?.Invoke(tar);
        }
        
        public void End()
        {
            onHurt -= onHurt;
            onDead -= onDead;
            mainGo.SetActive(false);
            if(dataCtrl!=null)
            {
                dataCtrl.Unload();
                dataCtrl = null;
            }
        }


        #endregion


        public void ShowAll()
        {
            foreach (var data in UnitForm.DataByUid.Values)
            {
                foreach (var data2 in FightUnitForm.DataByUid.Values)
                { 
                    
                }
                    data.unit.Show();
            }
        }

        public void UpdateInfo(UpdateSuperType superType)
        {
            List<Unit> cache = new List<Unit>();
            foreach (var data in UnitForm.DataByUid.Values)
                cache.Add(data.unit);

            
            foreach (var unit in cache)
            {
                bool passShowTest=false;
                switch ((UpdateType)unit.data.updateType)
                {
                    case UpdateType.Always:
                        passShowTest = true;
                        break;
                    case UpdateType.ShowOnly:
                        if (unit.isShowing)
                            passShowTest = true;
                        break;
                }

                if (!passShowTest)
                    continue;
                bool passSuperTest = false;
                switch (superType)
                {
                    case UpdateSuperType.All:
                        passSuperTest = true;
                        break;
                    case UpdateSuperType.NoSuperOnly:
                        if (unit.superUnit==null)
                            passSuperTest = true;
                        break;
                }
                if (!passSuperTest)
                    continue;
                unit.UpdateInfo();

            }
            
        }

    }
}
