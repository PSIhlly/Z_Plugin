using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Z_UnitSystem
{
public static class SaveAndLoad
    {
        static string perPath;
        static SaveAndLoad()
        {
            perPath= Application.persistentDataPath;
        }
        public static void Save(string key,string content)
        {
            File.WriteAllText((key.Contains(":")?"":(perPath + "/")) + key, content);
        }
        public static bool Exist(string key)
        {
            return File.Exists((key.Contains(":") ? "" : (perPath + "/")) + "/" + key);
        }
        public static string Load(string key)
        {
           return File.ReadAllText((key.Contains(":") ? "" : (perPath + "/")) + "/" + key);
        }
    }


}