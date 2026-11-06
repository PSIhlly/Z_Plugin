using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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

        // Preserve Package's filesystem JSON, but never retain all asset bytes,
        // a full UTF-16 JSON string and its UTF-8 copy at the same time.
        public static void WritePackage(string rootPath, Stream output, CancellationToken cancellationToken = default,
            Action<string, long, long> report = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            rootPath = GetRealPath(rootPath);
            long totalBytes = 0, completedBytes = 0;
            if (report != null)
                foreach (var file in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    totalBytes += new FileInfo(file).Length;
                }
            report?.Invoke("Package source bytes", 0, Math.Max(1, totalBytes));
            using (var text = new StreamWriter(output, new UTF8Encoding(false), 65536, true))
            using (var json = new JsonTextWriter(text) { Formatting = Formatting.Indented, CloseOutput = false })
            {
                var paths = new Queue<string>();
                paths.Enqueue(rootPath);
                json.WriteStartObject();
                while (paths.Count > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var path = paths.Dequeue();
                    foreach (var file in Directory.EnumerateFiles(path))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        json.WritePropertyName(file.Replace(rootPath, ""));
                        var bytes = File.ReadAllBytes(file);
                        // Keep a single JSON Base64 string, but feed compression
                        // in complete 3-byte blocks so large assets report progress too.
                        json.WriteRawValue("\"");
                        for (int offset = 0; offset < bytes.Length;)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            int count = Math.Min(49152, bytes.Length - offset);
                            text.Write(Convert.ToBase64String(bytes, offset, count));
                            text.Flush();
                            offset += count;
                            report?.Invoke("Package source bytes", Math.Min(completedBytes + offset, totalBytes), Math.Max(1, totalBytes));
                        }
                        text.Write('"');
                        completedBytes += bytes.Length;
                    }
                    foreach (var directory in Directory.EnumerateDirectories(path))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        paths.Enqueue(directory);
                        json.WritePropertyName(directory.Replace(rootPath, "/"));
                        json.WriteStartObject();
                        json.WriteEndObject();
                    }
                }
                json.WriteEndObject();
            }
            report?.Invoke("Package source bytes", Math.Max(1, totalBytes), Math.Max(1, totalBytes));
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
        public static void EachFile(string folder,Action<string> act)
        {
            folder = GetRealPath(folder);
            if (Exist(folder))
            {
                var lst = new List<string>(Directory.EnumerateFiles(folder));
                foreach(var file in lst)
                {
                    act(Path.GetFileName(file));
                }

            }
        }
        public static bool Exist(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
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
            // Absolute Core/asset paths must not be prefixed a second time;
            // neither the install path nor the filename is a product-name test.
            return Path.GetFullPath(Path.IsPathRooted(key) ? key : Path.Combine(perPath, key));
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
