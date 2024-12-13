using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;

namespace Z_Fight
{
    public class WeaponBullet
    {
        public int itemId_Data;
        public float damage;
        public int prefabId_Data;
        public int magazineCapacity;
        public float speed;
        public float range; 
        public Vector3 attackPos;
        public Vector3 attackDir;
        public bool selfHurt;
        public float accuracy;
        public int bulletsPer;
        public WeaponBullet(JObject jo)
        {
            LoadJsonData(jo);
        }
        public WeaponBullet(int itemId_Data, float damage, int prefabId_Data, int magazineCapacity,float speed, float range, Vector3 attackPos,Vector3 attackDir,bool selfHurt,float accuracy,int bulletsPer)
        {
            this.itemId_Data = itemId_Data;
            this.damage = damage;
            this.prefabId_Data = prefabId_Data;
            this.magazineCapacity = magazineCapacity;
            this.speed = speed;
            this.range = range;
            this.attackPos = attackPos;
            this.attackDir = attackDir;
            this.selfHurt = selfHurt;
            this.accuracy = accuracy;
            this.bulletsPer = bulletsPer;
        }

        public JObject GetJsonData()
        {
            JObject jo = new JObject();
            jo.Set("itemId_Data", itemId_Data);
            jo.Set("damage", damage);
            jo.Set("prefabId_Data", prefabId_Data);
            jo.Set("magazineCapacity", magazineCapacity);
            jo.Set("speed", speed);
            jo.Set("range",  range);
            jo.Set("attackPos", attackPos);
            jo.Set("attackDir", attackDir);
            jo.Set("selfHurt", selfHurt);
            jo.Set("accuracy",  accuracy);
            jo.Set("bulletsPer", bulletsPer);
            return jo;
        }
        private void LoadJsonData(JObject jo)
        {
            itemId_Data = jo.Get<int>("itemId_Data");
            damage = jo.Get<float>("damage");
            prefabId_Data = jo.Get<int>("prefabId_Data");
            magazineCapacity = jo.Get<int>("magazineCapacity");
            speed = jo.Get<float>("speed");
            range = jo.Get<float>("range");
            attackPos = jo.Get<Vector3>("attackPos");
            attackDir = jo.Get<Vector3>("attackDir");
            selfHurt = jo.Get<bool>("selfHurt");
            accuracy = jo.Get<float>("accuracy");
            bulletsPer = jo.Get<int>("bulletsPer");
        }

    }
    public class FightData
    {
        public int uidCnt;
        public List<FightUnit> fights;
        public List<WeaponUnit> weapons;
        public List<BulletUnit> bullets;
        public List<WeaponBullet> weaponBullets;

        
        public List<GameObject> prefabs;
        public List<Material> materials;
        
      
        public FightData(int uidCnt,List<FightUnit> fights,List<WeaponUnit> weapons,List<BulletUnit> bullets, List<WeaponBullet> weaponBullets, List<GameObject> prefabs, List<Material> materials)
        {
            this.uidCnt = uidCnt;
            this.fights = fights;
            this.weapons = weapons;
            this.bullets = bullets;
            this.prefabs = prefabs;
            this.materials = materials;
            this.weaponBullets = weaponBullets;
        }

        public FightData(JObject jo, List<GameObject> prefabs, List<Material> materials)
        {
            uidCnt = jo.Get<int>("uidCnt");
            
            this.prefabs = prefabs;
            this.materials = materials;

            var fightsJA = jo.Get<JArray>("fights");
            fights = new List<FightUnit>(fightsJA.Count);
            for (int i = 0; i < fightsJA.Count; i++)
            {
                fights.Add(new FightUnit((JObject)fightsJA[i]));
            }

            var weaponBulletsJA = jo.Get<JArray>("weaponBullets");
            weaponBullets = new List<WeaponBullet>(weaponBulletsJA.Count);
            for (int i = 0; i < weaponBulletsJA.Count; i++)
            {
                weaponBullets.Add(new WeaponBullet((JObject)weaponBulletsJA[i]));
            }

            var weaponsJA = jo.Get<JArray>("weapons");
            weapons = new List<WeaponUnit>(weaponsJA.Count);
            for (int i = 0; i < weaponsJA.Count; i++)
            {
                weapons.Add(new WeaponUnit((JObject)weaponsJA[i]));
            }

            var bulletsJA = jo.Get<JArray>("bullets");
            bullets = new List<BulletUnit>(bulletsJA.Count);
            for (int i = 0; i < bulletsJA.Count; i++)
            {
                bullets.Add(new BulletUnit((JObject)bulletsJA[i]));
            }

        }
        public JObject GetJsonData()
        {
            var jo = new JObject();
            jo.Set("uidCnt", uidCnt);


            var fightsJA = new JArray();
            for (int i = 0; i < fights.Count; i++)
            {
                fightsJA.Add(fights[i].GetJsonData());
            }
            jo.Set("fights", fightsJA);

            var weaponBulletsJA = new JArray();
            for (int i = 0; i < weaponBullets.Count; i++)
            {
                weaponBulletsJA.Add(weaponBullets[i].GetJsonData());
            }
            jo.Set("weaponBullets", weaponBulletsJA);

            var weaponsJA = new JArray();
            for (int i = 0; i < weapons.Count; i++)
            {
                weaponsJA.Add(weapons[i].GetJsonData());
            }
            jo.Set("weapons", weaponsJA);

            var bulletsJA = new JArray();
            for (int i = 0; i < bullets.Count; i++)
            {
                bulletsJA.Add(bullets[i].GetJsonData());
            }
            jo.Set("bullets", bulletsJA);
            return jo;
        }
        public static FightData GetDefault(List<GameObject> prefabs, List<Material> materials)
        {
            Vector3[] poss = new Vector3[] { new Vector3(0, 0, 0), new Vector3(5, 0, 5) };
            int uidCnt = 0;
            var fights = new List<FightUnit>();
            var weapons = new List<WeaponUnit>();
            var bullets = new List<BulletUnit>();

            var weaponBullets = new List<WeaponBullet>();
            for (int i = 0; i < poss.Length; i++)
            {
                fights.Add(new FightUnit(++uidCnt, 0, poss[i], new Vector3(0,0,0), Vector3.one,0,i==0,5,new Dictionary<int, int> { {0,100 } },new List<int>() {0 },new List<int>(),100,100,1,0));
            }
            for (int i = 0; i < 1; i++)
            {
                weaponBullets.Add(new WeaponBullet(i,10,2,30,3,10,Vector3.zero,Vector3.zero,false,0.95f,1));
            }
            for (int i = 0; i < poss.Length; i++)
            {
                weapons.Add(new WeaponUnit(++uidCnt, i+1,1, poss[i], new Vector3(0, 0, 0), Vector3.one,0,new List<int>() {0},0.2f,0,5,10));
            }
            


            return new FightData(uidCnt, fights, weapons, bullets, weaponBullets, prefabs, materials);
            //new Vector3(1, 3, 1),new Vector3Int(5, 2, 5),maps,items,characters
        }
        public void Unload()
        {
           foreach(var fight in fights)
            {
                fight.Hide();
            }
            foreach (var weapon in weapons)
            {
                weapon.Hide();
            }
            foreach (var bullet in bullets)
            {
                bullet.Hide();
            }
        }
    }
}
