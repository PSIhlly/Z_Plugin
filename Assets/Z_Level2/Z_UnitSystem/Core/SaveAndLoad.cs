using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Z_UnitSystem
{
public static class SaveAndLoad
    {
        static string path;
        static SaveAndLoad()
        {
            path= Application.persistentDataPath; ;
        }
        public static void Save(string key,string content)
        {
            File.WriteAllText(path + "/" + key, content);
        }
        public static bool Exist(string key)
        {
            return File.Exists(path + "/" + key);
        }
        public static string Load(string key)
        {
           return File.ReadAllText(path + "/" + key);
        }
    }


}