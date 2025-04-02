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
        

        

        public PlayData(string formData)
        {

        progress = ProgressForm.GetDataByJo(JObject.Parse(formData));

        }
        public JObject GetJsonData()
        {
            return ProgressForm.GetJoByData(progress);
        }
        public PlayData()
        {
            progress = new ProgressForm.Data(1,1,Vector3.zero);
        }
        public void Unload()
        {
         
        }
       
       
    }

