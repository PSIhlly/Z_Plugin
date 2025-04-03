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
            var path = GetRealPath(key);
            Build(path);
            File.WriteAllBytes(path, content);
        }
        public static void Save(string key,string content)
        {
            var path = GetRealPath(key);
            Build(path);
            File.WriteAllText(path, content);
        }
        public static void Build(string path)
        {
            string directoryPath = Path.GetDirectoryName(path);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(path))
            {
                var f = File.Create(path);
                f.Close();
            }
        }
        public static bool Exist(string key)
        {
            var path = GetRealPath(key);
            if (Directory.Exists(path))
                return true;
            return File.Exists(path);
        }
        public static T Load<T>(string key)
        {
            var path = GetRealPath(key);
            if(Exist(path))
            {
            if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)File.ReadAllBytes(path);
            }
            else
            {
                return (T)(object)File.ReadAllText(path);
                }

            }
            return (T)(object)null;
        }
        private static string GetRealPath(string key)
        {
            return Path.GetFullPath((key.Contains("HlZy") ? "" : (perPath + "/")) + key);
        }
        public static void Copy(string from, string to)
        {
            to = GetRealPath(to);
            from = GetRealPath(from);
            Build(to);
            File.Copy(from, to);
        }
    }


}