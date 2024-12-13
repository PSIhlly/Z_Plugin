using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Z_UnitSystem
{
public static class SaveAndLoad
{
        static string path => Application.persistentDataPath;
        public static void Save(string key,string content)
        {
            File.WriteAllText(path + "/" + key, content);
        }
        public static string Load(string key)
        {
           return File.ReadAllText(path + "/" + key);
        }
    }


}