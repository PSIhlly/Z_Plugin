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
        public static void Save(string key, byte[] content)
        {
            File.WriteAllBytes((key.Contains("HlZy") ? "" : (perPath + "/")) + key, content);
        }
        public static void Save(string key,string content)
        {
            File.WriteAllText((key.Contains("HlZy")?"":(perPath + "/")) + key, content);
        }
        public static bool Exist(string key)
        {
            return File.Exists((key.Contains("HlZy") ? "" : (perPath + "/")) + "/" + key);
        }
        public static T Load<T>(string key)
        {
            if(typeof(T) == typeof(byte[]))
            {
                return (T)(object)File.ReadAllBytes((key.Contains("HlZy") ? "" : (perPath + "/")) + "/" + key);
            }
            else
            {
                return (T)(object)File.ReadAllText((key.Contains("HlZy") ? "" : (perPath + "/")) + "/" + key);
            }
        }
        
    }


}