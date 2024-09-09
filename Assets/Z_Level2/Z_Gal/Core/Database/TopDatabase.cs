using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Gal.Database
{
    public class TopDatabase: Z_Database
    {
        #region upDatabase

        #endregion

        public class RawTopData:RawData
        {
            public ClipDatabase clipDatabase;
            public MainTextDatabase mainTextDatabase;

        }
        Dictionary<int, RawTopData> dataDic = new Dictionary<int, RawTopData>();
        public override RawData GetRawData(int id)
        {
            return dataDic[id];
        }


        public override bool Load(bool loadSubDatabase)
        {

            if (!base.Load(loadSubDatabase))
                return false;
           
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
