using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Gal.Database
{
    public class ClipDatabase: Z_Database
    {
   

        public class RawClipData:RawData
        {
            public int id_inMainTextDatabase;
            public int id_inMainPictureDatabase;
            public RawClipData(ClipDatabase database,int id,int id_inMainTextDatabase,int id_inMainPictureDatabase)
            {
                this.database = database;
                this.id = id;
                this.id_inMainTextDatabase = id_inMainTextDatabase;
                this.id_inMainPictureDatabase = id_inMainPictureDatabase;
            }
            public string GetMainText()
            {
                var mdatabase=((TopDatabase.RawTopData)database.upData).mainTextDatabase;
                return ((MainTextDatabase.RawMainTextData)mdatabase.GetRawData(id_inMainTextDatabase)).text;
                //MainTextDatabase.RawMainTextData rawMainTextData = ().//();
            }

        }
        Dictionary<int, RawClipData> dataDic = new Dictionary<int, RawClipData>();
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
            JArray ja = (JArray)jo["clip"];

            for (int i=0;i< ja.Count;i++)
            {
                JObject nowJo =(JObject) ja[i];
                dataDic[(int)nowJo["id"]]=new RawClipData(this,(int)nowJo["id"], (int)nowJo["idInMainTextDatabase"], (int)nowJo["idInMainPictureDatabase"] );
            }
            return false;
        }

    }
    /*json style:
    "clip"([])
        "id"(int)
        "idInMainTextDatabase"(int)
        "idInMainPictureDatabase"(int)

    */

}
