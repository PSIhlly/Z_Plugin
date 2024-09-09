using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Z_DesignStyle;
using Z_TileMap.Map;
namespace Z_TileMap.Core
{
    public class UnitDatabase : Z_Database
    {
   
        // Start is called before the first frame update
        public class RawUnitData:RawData
        {
            public int id_inAttributeData;
            public int[] connectedUnitMapsId;
            public int belong;
            public int[] coordinates;

            public RawUnitData(UnitDatabase database,int id, int id_inAttributeData,int[] connectedUnitMapsId,int belong,int[] coordinates)
            {
                this.database = database;
                this.id = id;
                this.id_inAttributeData = id_inAttributeData;
                this.connectedUnitMapsId = connectedUnitMapsId;
                this.belong = belong;
                this.coordinates = coordinates;
            }
        }
        Dictionary<int, RawUnitData> dataDic = new Dictionary<int, RawUnitData>();



        public override bool Load(bool loadSubDatabase)
        {

            if (!base.Load(loadSubDatabase))
                return false;
            string rawString = File.ReadAllText(address + fileName);
            JObject jo = JObject.Parse(rawString);
            JArray ja = (JArray)jo["tileMap"];

            for (int i = 0; i < ja.Count; i++)
            {
                JObject nowJo = (JObject)ja[i];
                JArray connectedUnitMapsId_Ja = (JArray)nowJo["connectedUnitMapsId"];
                int[] connectedUnitMapsId = new int[connectedUnitMapsId_Ja.Count];
                for (int j=0;j< connectedUnitMapsId_Ja.Count;j++)
                {
                    connectedUnitMapsId[j] = (int)connectedUnitMapsId_Ja[j];
                }

                JArray coordinates_Ja = (JArray)nowJo["coordinates"];
                int[] coordinates = new int[coordinates_Ja.Count];
                for (int j = 0; j < coordinates_Ja.Count; j++)
                {
                    coordinates[j] = (int)coordinates_Ja[j];
                }
                dataDic[(int)nowJo["id"]] = new RawUnitData(this,(int)nowJo["id"], (int)nowJo["id_inAttributeData"], connectedUnitMapsId, (int)nowJo["belong"], coordinates);
            }
            return true;

        }
    }
    /*json style:
    "tileMap"([])
        "id"(int)
        "id_inAttributeData"(int)
        "connectedUnitMapsId"(int[])
        "belong"(int)
        "coordinates"(int[])

    */
}
