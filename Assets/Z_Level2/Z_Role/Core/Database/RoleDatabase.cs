using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Role.Core
{
    public class RoleDatabase : Z_Database
    {
        public class RawRoleData : RawData
        {
            public int profilePhotoTextureId_inTextureDatabase;
            public RawRoleData(RoleDatabase database,int id, int profilePhotoTextureId_inTextureDatabase)
            {
                this.database = database;
                this.id = id;
                this.profilePhotoTextureId_inTextureDatabase = profilePhotoTextureId_inTextureDatabase;
            }
        }
        Dictionary<int, RawRoleData> dataDic = new Dictionary<int, RawRoleData>();
        public override RawData GetRawData(int id)
        {
            return dataDic[id];
        }

        public override bool Load(bool loadSubDatabase)
        {

            if (!base.Load(loadSubDatabase))
                return false;
            string rawString = File.ReadAllText(address + fileName);
            JObject jo = JObject.Parse(rawString);
            JArray ja = (JArray)jo["texture"];

            for (int i = 0; i < ja.Count; i++)
            {
                JObject nowJo = (JObject)ja[i];
                dataDic[(int)nowJo["id"]] = new RawRoleData(this,(int)nowJo["id"], (int)nowJo["profilePhotoTextureId_inTextureDatabase"]);
            }
            return true;
        }

    }
    /*json style:
    "role"([])
        "id"(int)
        "profilePhotoTextureId_inTextureDatabase"(int)

    */
}