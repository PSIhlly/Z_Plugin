using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Z_Texture
{
    public static class TextureHelper
    {


        private static Texture2D _transparentTexture;
        public static Texture2D transparentTexture
        {
            get 
            {
                if (_transparentTexture == null)
                {
                    _transparentTexture = new Texture2D(5, 5);
                    for (int y = 0; y < 5; y++)
                        for (int x = 0; x < 5; x++)
                        {
                            _transparentTexture.SetPixel(x, y, new Color(0,0,0,1));
                        }
                }
                return _transparentTexture; 
            }
        }

        public static Texture GetTextureByPath(string path)
        {
            Texture res;
            if(File.Exists(path))
            {
                res = InternalGetTextureByPath(path);
            }
            else
            {
                res = InternalGetTextureByPathWithoutExtension(path);
            }
            return res;
        }
        public static Texture GetTextureByByte(byte[] data)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);
            return texture;
        }
        public static Sprite GetSpriteByPath(string path)
        {
                var tex = GetTextureByPath(path);
                var s = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                return s;
        }
        public static Sprite GetSpriteByTexture(Texture tex)
        {
            return Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        public static byte[] GetTextureByte(Texture2D tex)
        {
            return tex.EncodeToPNG();
        }
        
        

        public static void DeleteTexture(string path)
        {
            File.Delete(path);
        }

        public static void RenameTexture(string path,string oldFileName,string newFileName)
        {
            var oldPath = Path.GetFullPath(path+"/"+ oldFileName);
            var newPath = Path.GetFullPath(path+"/"+ newFileName);
            File.Move(oldPath, newPath);
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