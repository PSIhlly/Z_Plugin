using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Gal.Database
{
    public class MainTextDatabase : Z_Database
    {
        public class RawMainTextData : RawData
        {
            public string text;
            public RawMainTextData(MainTextDatabase database,int id, string text)
            {
                this.database = database;
                this.id = id;
                this.text = text;
            }
        }
        Dictionary<int, RawMainTextData> dataDic = new Dictionary<int, RawMainTextData>();
        public override RawData GetRawData(int id)
            {
            return dataDic[id];
            }

        public override bool Load(bool loadSubDatabase)
        {

            if (!base.Load(loadSubDatabase))
                return false;
            string rawString = File.ReadAllText(address + fileName);
            JObject jo= JObject.Parse(rawString);
            JArray ja = (JArray)jo["mainText"];
            for (int i=0;i< ja.Count;i++)
            {
                JObject nowJo = (JObject)ja[i];
                dataDic[(int)nowJo["id"]] = new RawMainTextData(this,(int)nowJo["id"], (string)nowJo["text"]);
            }
            return false;
        }

    }
    /*json style:
    "mainText"([])
        "id"(int)
        "text"(string)
    



    */

}
