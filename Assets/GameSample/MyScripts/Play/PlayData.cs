using Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_ByteSerialize;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_Debug;
using Z_DesignStyle;
using Z_UnitSystem.Form;

    public class PlayData
    {
        public ProgressForm.Data progress;




    public PlayData(string progressData)
        {

        progress = ProgressForm.GetDataByJo(JObject.Parse(progressData));

    }
    public PlayData(ProgressForm.Data progressData)
    {
        progress = progressData;
    }
    public JObject GetJsonData()
        {
            return ProgressForm.GetJoByData(progress);
        }
        public PlayData()
        {
        Debug.LogError("No character!!!");
        string nm = "";
        foreach(var c in CharacterProductForm.DataByName.Values)
        {
            nm = c.name;
        }
            progress = new ProgressForm.Data(1,1,Vector3.zero, nm);
        }
        public void Unload()
        {
         
        }
       
       
    }

