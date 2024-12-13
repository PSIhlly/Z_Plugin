using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_UnitSystem;

namespace Z_Fight
{
    public enum UpdateSuperType
    {
        All,
        NoSuperOnly
    }
    public class FightManager : Z_MonoManager<FightManager>
    {
        public FightData data;
        public GameObject mainGo;

        public Dictionary<int, Unit> unitDic = new Dictionary<int, Unit>();

        public Action<BulletUnit, Unit> onHurt;

        public Action<FightUnit> onDead;

        public override void Init()
        {
            base.Init();
        }

        #region external

        public void Begin(FightData data)
        {
            End();
            Init();

            mainGo.SetActive(true);
            this.data = data;
            for (int i = 0; i < data.fights.Count; i++)
            {
                Register(data.fights[i]);
            }
            for (int i = 0; i < data.weapons.Count; i++)
            {
                CheckAndLoad(data.weapons[i]);
            }

        }
        private bool CheckAndLoad(Unit unit)
        {
            Register(unit);
            if (unit is WeaponUnit weapon)
            {
                unitDic[weapon.fightUid_Data].Bind(unit);
                return true;
            }
            return false;
        }
        public void AddUnit(Unit unit)
        {
            if (unit is FightUnit fight)
            {
                data.fights.Add(fight);
            }
            else if (unit is WeaponUnit weapon)
            {
                data.weapons.Add(weapon);
            }
            else if (unit is BulletUnit bullet)
            {
                data.bullets.Add(bullet);
            }
            CheckAndLoad(unit);
            unit.UpdateActive();
        }
        public void RemoveUnit(int uid)
        {
            var unit = unitDic[uid];
            if (unit is FightUnit fight)
            {
                data.fights.Remove(fight);
            }
            else if (unit is WeaponUnit weapon)
            {
                data.weapons.Remove(weapon);
            }
            else if (unit is BulletUnit bullet)
            {
                data.bullets.Remove(bullet);
            }
            if(unit.superUnit!=null)
                unit.superUnit.Unbind(unit);
            unit.VisOff();
            unit.Hide();
            unitDic.Remove(uid);
        }
        public void SetTarget(FightUnit self, int tar)
        {
            self.SetTarget(tar);
        }
        public void Hurt(BulletUnit bullet, Unit tar)
        {
            if(tar is FightUnit fight)
            {
                var hurt = bullet.weaponBullet.damage - fight.defence;
                if (hurt < 0) hurt = 0;
                fight.SetHp(fight.hp- hurt);
            }
            onHurt?.Invoke(bullet, tar);
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
            if(data!=null)
            {
                data.Unload();
                data = null;
            }

        }


        #endregion

        private void Register(Unit tar)
        {
            unitDic[tar.uid] = tar;
        }
        private void Unregister(Unit tar)
        {
            if (unitDic.ContainsKey(tar.uid))
            {
                unitDic.Remove(tar.uid);
            }
        }
        public void ShowAll()
        {
            foreach (var u in unitDic.Values)
                u.Show();
        }

        public void UpdateInfo(UpdateSuperType superType)
        {
            List<Unit> cache = new List<Unit>();
            foreach (var u in unitDic.Values)
                cache.Add(u);

            
            foreach (var unit in cache)
            {
                bool passShowTest=false;
                switch (unit.updateType)
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
