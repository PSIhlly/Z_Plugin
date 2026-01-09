using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Z_DataSystem;

namespace Z_UnitSystem
{
    public static class SaveAndLoad
    {
        public static string perPath = Application.persistentDataPath;
        static SaveAndLoad()
        {
        }
        public static string Package(string rootPath)
        {
            rootPath = GetRealPath(rootPath);
            Dictionary<string, JObject> joDic = new Dictionary<string, JObject>();
            JObject root = new JObject();
            Queue<string> pathQueue = new Queue<string>();
            pathQueue.Enqueue(rootPath);
            joDic[rootPath] = root;
            while (pathQueue.Count > 0)
            {
                var path = pathQueue.Dequeue();
                var jo = joDic[path];
                foreach (var file in Directory.EnumerateFiles(path))
                {
                    var key = file.Replace(rootPath, "");
                    root[key] = File.ReadAllBytes(file);
                }
                foreach (var subPath in Directory.EnumerateDirectories(path))
                {
                    pathQueue.Enqueue(subPath);
                    joDic[subPath] = new JObject();
                    var key = subPath.Replace(rootPath, "/");
                    root[key] = joDic[subPath];
                }
            }
            return root.ToString();
        }
        public static void Unpackage(string content, string rootPath)
        {
            rootPath = GetRealPath(rootPath);
            var root = JObject.Parse(content);

            Queue<JObject> joQueue = new Queue<JObject>();
            joQueue.Enqueue(root);
            while (joQueue.Count > 0)
            {
                var jo = joQueue.Dequeue();
                foreach (var subJo in jo)
                {
                    if (subJo.Key.StartsWith("/"))
                    {
                        joQueue.Enqueue((JObject)subJo.Value);
                    }
                    else
                    {
                        var key = rootPath + subJo.Key;
                        Save(key, (byte[])subJo.Value);
                    }
                }
            }
        }
        public static void Save(string key, byte[] content)
        {
            var path = GetRealPath(key);
            Build(path);
            File.WriteAllBytes(path, content);
        }
        public static void Save(string key, string content)
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
            if (Exist(path))
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
        public static void Delete(string path)
        {
            path = GetRealPath(path);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        public static string GetRealPath(string key)
        {
            return Path.GetFullPath((key.Contains("HlZy") ? "" : (perPath + "/")) + key);
        }
        public static void Copy(string from, string to)
        {
            Debug.Log(to);
            to = GetRealPath(to);
            from = GetRealPath(from);
            Build(to);
            File.Copy(from, to, true);
        }
    }


}