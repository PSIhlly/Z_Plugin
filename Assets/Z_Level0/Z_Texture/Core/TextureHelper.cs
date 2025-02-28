using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Z_Texture
{
    public static class TextureHelper
    {
        private static Dictionary<string,Texture> textureCache=new Dictionary<string, Texture>();
        private static Dictionary<Texture, Sprite> spriteCache = new Dictionary<Texture, Sprite>();
        public static Texture GetTextureByPath(string path)
        {
            path = Path.GetFullPath(path);
            if (textureCache.ContainsKey(path))
                return textureCache[path];
            Texture res;
            if(File.Exists(path))
            {
                res = InternalGetTextureByPath(path);
            }
            else
            {
                res = InternalGetTextureByPathWithoutExtension(path);
            }
            textureCache[path] = res;
            return res;
        }
        public static Texture GetTextureByByte(byte[] data)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            return texture;
        }
        public static Sprite GetSpriteByPath(string path,string basePath="")
        {
            if(string.IsNullOrEmpty(basePath))
            {
                basePath = Application.dataPath;
            }
            path = basePath + path;
            Texture tex;
            if (textureCache.ContainsKey(path))
            {
                tex = textureCache[path];
            }else
            {
                tex = GetTextureByPath(path);
            }

            if (spriteCache.ContainsKey(tex))
            {
                return spriteCache[tex];
            }
            else
            {
                var s = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                spriteCache[tex] = s;
                return s;
            }
        }
        public static Sprite GetSpriteByTexture(Texture tex)
        {
            if (spriteCache.ContainsKey(tex))
                return spriteCache[tex];
            var s = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            spriteCache[tex] = s;
            return s;
        }
        public static void SaveTexture(Texture2D tex, string folder, string fileName)
        {
            SaveTexture(tex.EncodeToPNG(), folder, fileName);
        }
        public static void SaveTexture(byte[] data,string folder,string fileName)
        {
            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllBytes(folder+fileName, data);
        }

        public static void DeleteTexture(string path)
        {
            path = Path.GetFullPath(path);
            if (textureCache.ContainsKey(path))
            {
                var tex = textureCache[path];
                File.Delete(path);
                if (spriteCache.ContainsKey(tex))
                {
                    spriteCache.Remove(tex);
                }
                textureCache.Remove(path);
            }
        }

        #region util
        private static Texture InternalGetTextureByPath(string path)
        {
            // 尝试获取文件的字节数组
            if (File.Exists(path))
            {
                return GetTextureByByte(File.ReadAllBytes(path));
            }
            Debug.LogError("文件未找到：" + path);
            return Texture2D.whiteTexture;
        }
        private static Texture InternalGetTextureByPathWithoutExtension(string path)
        {

            // 尝试获取文件的字节数组
            foreach (var extension in new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }) // 添加你需要的后缀
            {
                string fileWithExtension = path + extension;

                if (File.Exists(fileWithExtension))
                {
                    return InternalGetTextureByPath(fileWithExtension);
                }
            }

            Debug.LogError("文件未找到：" + path);
            return Texture2D.whiteTexture;
        }

        

        #endregion
    }


}