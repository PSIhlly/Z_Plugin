using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using Z_ByteSerialize;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;
using static System.Net.Mime.MediaTypeNames;
namespace Z_DataSystem.Form
{
    public partial class TexAssetForm
    {
        public partial class Data
        {
            private Sprite _sprite;
            private Texture _texture => (Texture)asset;
            Z_MultiTask<Texture> texTask = new Z_MultiTask<Texture>();
            Z_MultiTask<Sprite> spriteTask = new Z_MultiTask<Sprite>();
            public void GetTexAsync(Action<Texture> onLoaded)
            {
                texTask.Run(_texture, GetTex, onLoaded);
            }
            public Texture GetTex()
            {
                if (_texture == null)
                {
                    if (bytes == null)
                    {
                        asset = TextureHelper.GetTextureByPath(path);
                    }
                    else
                    {
                        asset = TextureHelper.GetTextureByByte(bytes);
                    }
                }
                return _texture;
            }

            public void GetSpriteAsync(Action<Sprite> onLoaded)
            {
                spriteTask.Run(_sprite, GetSprite, onLoaded);
            }
            public Sprite GetSprite()
            {
                if (_sprite == null)
                {
                    _sprite = TextureHelper.GetSpriteByTexture(GetTex());
                }
                return _sprite;
            }
        }
    }
}
namespace Z_DataSystem
{
    public class TexController : Z_Controller<AssetManager>, IAssetController
    {

        public bool IsAsset(string name)
        {
            var parts = name.Split(GetMark());
            return parts.Length == 3 && string.IsNullOrEmpty(parts[0]) && string.IsNullOrEmpty(parts[2]);
        }
        public string GetName(string name = "")
        {
            return $"{GetMark()}{name}{GetMark()}";
        }
        public string GetMark() => AssetDefines.IMAGE_MARK;
        public string[] GetSupportedExtensions() => new[]
{
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp"
        };

        public TexController(AssetManager super) : base(super)
        {
        }


        public class SelectTexTask : SelectTask<TexController>
        {
            public Action<TexAssetForm.Data> callback;
            public Vector2Int forceSize;
            public override void Run(TexController ctrl)
            {
                base.Run(ctrl);
                NativeGallery.GetImageFromGallery((path)=> OnImportComplete(string.IsNullOrEmpty(path)?null:File.ReadAllBytes(path)));
            }
            public override void OnImportComplete(byte[] data)
            {
                if (data != null)
                {
                    var tex = (Texture2D)TextureHelper.GetTextureByByte(data);
                    if (forceSize != Vector2Int.zero)
                        tex = TextureTransform.GetTargetSize(tex, forceSize.x, forceSize.y);
                    var newBytes = TextureHelper.GetTextureByte(tex);
                    var nm = ctrl.GetName(BytesSerialize.GetHash(newBytes));
                    var form = ctrl.CreateDataByBytes(newBytes, nm);
                    callback?.Invoke(form);
                    Z_EventHelper.Invoke(new AssetEvent()
                    {
                        importAssetName = nm
                    });
                }
            }
        }
        public List<string> GetTexAssetsByFolder(string path, bool isRes)
        {
            List<string> res = new List<string>();
            if (isRes)
            {
                Texture2D[] textures = Resources.LoadAll<Texture2D>(path);
                foreach (var tex in textures)
                {
                    res.Add(tex.name);
                }

            }
            else
            {
                string[] allFiles = Directory.GetFiles(path);
                // 过滤出图片文件
                foreach (string file in allFiles)
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (Array.Exists(GetSupportedExtensions(), ext => ext == extension))
                    {
                        res.Add(file);
                    }
                }
            }
            return res;
        }

        public void Select(Vector2Int forceSize = default, Action<TexAssetForm.Data> callback = null)
        {
            SelectTexTask task = new SelectTexTask();
            task.callback = callback;
            task.forceSize = forceSize;
            task.Run(this);
        }
        public TexAssetForm.Data CreateDataByTex(Texture2D tex, string name, Vector2Int forceSize)
        {
            return CreateDataByBytes(TextureTransform.GetTargetSize(tex, forceSize.x, forceSize.y).EncodeToPNG(), name);
        }
        public TexAssetForm.Data CreateDataByTex(Texture2D tex, string name)
        {
            return new TexAssetForm.Data(-1, name, "", null, "", tex);
        }
        public TexAssetForm.Data CreateDataByBytes(byte[] data, string name)
        {
            return new TexAssetForm.Data(-1, name, "", data, BytesSerialize.GetHash(data), null);
        }
        public TexAssetForm.Data CreateDataByPath(string path, string name)
        {
            path = SaveAndLoad.GetRealPath(path);
            return new TexAssetForm.Data(-1, name, path, null, "", null);
        }
        public TexAssetForm.Data CreateDataByTex(Texture tex, string name)
        {
            return CreateDataByTex((Texture2D)tex, name);
        }

    }
}
