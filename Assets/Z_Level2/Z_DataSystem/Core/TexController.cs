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
            private List<GifFrameData> _gifFrames;
            private List<Sprite> _gifSprites;
            Z_MultiTask<Texture> texTask = new Z_MultiTask<Texture>();
            Z_MultiTask<Sprite> spriteTask = new Z_MultiTask<Sprite>();

            public bool isGif
            {
                get
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                            return true;
                        byte[] buffer = new byte[3];
                        using (FileStream fs = new FileStream(
                            path,
                        FileMode.Open,
                        FileAccess.Read,
                    FileShare.Read,
                    4096,
                        FileOptions.SequentialScan)) 
                        {
                            fs.Read(buffer, 0, 3);
                        }
                        if (buffer[0] == 'G' && buffer[1] == 'I' && buffer[2] == 'F')
                            return true;
                    }

                    if (bytes != null && bytes.Length > 3 && bytes[0] == 'G' && bytes[1] == 'I' && bytes[2] == 'F')
                        return true;
                    return false;
                }
            }

            public List<GifFrameData> GetGifFrames()
            {
                if (_gifFrames == null && isGif)
                {
                    if (bytes != null)
                        _gifFrames = TextureHelper.GetGifFramesByByte(bytes);
                    else if (path != null)
                        _gifFrames = TextureHelper.GetGifFramesByPath(path);
                }
                return _gifFrames;
            }

            public List<Sprite> GetGifSprites()
            {
                if (_gifSprites == null)
                {
                    var frames = GetGifFrames();
                    if (frames != null)
                    {
                        _gifSprites = new List<Sprite>(frames.Count);
                        foreach (var frame in frames)
                        {
                            _gifSprites.Add(TextureHelper.GetSpriteByTexture(frame.texture));
                        }
                    }
                }
                return _gifSprites;
            }

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
                    else if (path != null)
                    {
                        asset = TextureHelper.GetTextureByByte(bytes);
                    }
                    else
                    {
                        asset = TextureHelper.transparentTexture;
                    }
                }
                return _texture;
            }
            public byte[] GetBytes()
            {

                if (bytes == null && path != null)
                {
                    bytes = File.ReadAllBytes(path);
                }
                return bytes;
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
        public string GetName(int id = -1)
        {
            return $"{GetMark()}{(id>0?id.ToString():"")}{GetMark()}";
        }
        public int GetId(string name = "")
        {
            if (name == null)
                return -1;
            var parts = name.Split(GetMark());
            if(parts.Length == 3&& int.TryParse(parts[1], out int id))
            {
                return id;
            }
            return -1;
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
                NativeGallery.GetImageFromGallery((path) => OnImportComplete(string.IsNullOrEmpty(path) ? null : File.ReadAllBytes(path), Path.GetFileNameWithoutExtension(path)));
            }
            public override void OnImportComplete(byte[] data,string name)
            {
                if (data != null)
                {
                    var tex = (Texture2D)TextureHelper.GetTextureByByte(data);
                    if (forceSize != Vector2Int.zero)
                        tex = TextureTransform.GetTargetSize(tex, forceSize.x, forceSize.y);
                    var newBytes = data;
                    var form = ctrl.CreateDataByBytes(newBytes, name);
                    callback?.Invoke(form);
                    Z_EventHelper.Invoke(new AssetEvent()
                    {
                        importAssetName = name
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
                // ���˳�ͼƬ�ļ�
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
            return new TexAssetForm.Data(-1, name, "", null, "", tex, "");
        }
        public TexAssetForm.Data CreateDataByBytes(byte[] data, string name)
        {
            return new TexAssetForm.Data(-1, name, "", data, BytesSerialize.GetHash(data), null, "");
        }
        public TexAssetForm.Data CreateDataByPath(string path, string name)
        {
            path = SaveAndLoad.GetRealPath(path);
            return new TexAssetForm.Data(-1, name, path, null, "", null, "");
        }
        public TexAssetForm.Data CreateDataByTex(Texture tex, string name)
        {
            return CreateDataByTex((Texture2D)tex, name);
        }

    }
}
