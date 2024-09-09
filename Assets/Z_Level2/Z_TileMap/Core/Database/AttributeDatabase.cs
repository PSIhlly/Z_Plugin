using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Z_DesignStyle;
namespace Z_TileMap.Core
{
    public class AttributeDatabase : Z_Database
{
    public class RawAttributeData : RawData
    {
        public int id_inTextureDatabase;
        public RawAttributeData(AttributeDatabase database,int id, int id_inTextureDatabase)
        {
                this.database = database;
            this.id = id;
            this.id_inTextureDatabase = id_inTextureDatabase;
        }
    }
    Dictionary<int, RawAttributeData> dataDic = new Dictionary<int, RawAttributeData>();
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
        JArray ja = (JArray)jo["attribute"];
        for (int i = 0; i < ja.Count; i++)
        {
            JObject nowJo = (JObject)ja[i];
            dataDic[(int)nowJo["id"]] = new RawAttributeData(this,(int)nowJo["id"], (int)nowJo["id_inTextureDatabase"]);
        }
            return true;
    }

}

    /*json style:
        "attribute"([])
            "id"(int)
            "id_inTextureDatabase"(int)

        */
}
