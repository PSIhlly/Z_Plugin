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
            private List<AnimatedFrameData> _webPFrames;
            private List<Sprite> _webPSprites;
            private bool animationFramesResolved;
            private IReadOnlyList<AnimatedFrameData> animationFrames;
            private double[] animationFrameEnds;
            Z_MultiTask<Texture> texTask = new Z_MultiTask<Texture>();
            Z_MultiTask<Sprite> spriteTask = new Z_MultiTask<Sprite>();

            public void ClearRuntimeCache()
            {
                asset = null;
                _sprite = null;
                _gifFrames = null;
                _gifSprites = null;
                _webPFrames = null;
                _webPSprites = null;
                animationFramesResolved = false;
                animationFrames = null;
                animationFrameEnds = null;
            }

            public bool isGif
            {
                get
                {
                    if (bytes != null)
                        return bytes.Length >= 3 && bytes[0] == 'G' && bytes[1] == 'I' && bytes[2] == 'F';
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
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

            public bool isWebP
            {
                get
                {
                    if (bytes != null)
                        return TextureHelper.IsWebP(bytes);
                    if (string.IsNullOrEmpty(path))
                        return false;
                    if (path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (!File.Exists(path))
                        return false;
                    var header = new byte[12];
                    using (var stream = File.OpenRead(path))
                        return stream.Read(header, 0, header.Length) == header.Length && TextureHelper.IsWebP(header);
                }
            }

            public IReadOnlyList<AnimatedFrameData> GetAnimationFrames()
            {
                if (animationFramesResolved)
                    return animationFrames;
                if (isGif)
                    animationFrames = GetGifFrames();
                else if (isWebP)
                {
                    if (_webPFrames == null)
                        _webPFrames = bytes != null
                            ? TextureHelper.GetWebPFramesByByte(bytes)
                            : TextureHelper.GetWebPFramesByPath(path);
                    animationFrames = _webPFrames;
                }
                // Cache static assets too: material animation must not open a file every frame.
                animationFramesResolved = true;
                return animationFrames;
            }

            public void SetRuntimeAnimationFrames(IReadOnlyList<AnimatedFrameData> frames)
            {
                ClearRuntimeCache();
                animationFrames = frames;
                animationFramesResolved = true;
                if (frames != null && frames.Count > 0)
                    asset = frames[0].texture;
            }

            public double GetAnimationDuration()
            {
                var frames = GetAnimationFrames();
                if (frames == null || frames.Count == 0)
                    return 0;
                if (animationFrameEnds == null)
                {
                    animationFrameEnds = new double[frames.Count];
                    double end = 0;
                    for (int i = 0; i < frames.Count; i++)
                    {
                        float delay = frames[i].delaySeconds;
                        end += delay > 0 && !float.IsInfinity(delay) ? delay : .1f;
                        animationFrameEnds[i] = end;
                    }
                }
                return animationFrameEnds[animationFrameEnds.Length - 1];
            }

            public Texture GetAnimationTexture(double seconds)
            {
                if (GetAnimationDuration() <= 0)
                    return GetTex();
                // Past the internal cycle, hold its last frame for the clip interval.
                int lo = 0, hi = animationFrameEnds.Length - 1;
                while (lo < hi)
                {
                    int mid = (lo + hi) / 2;
                    if (seconds < animationFrameEnds[mid]) hi = mid;
                    else lo = mid + 1;
                }
                return animationFrames[lo].texture;
            }

            public IReadOnlyList<Sprite> GetAnimationSprites()
            {
                if (isGif)
                    return GetGifSprites();
                var frames = GetAnimationFrames();
                if (frames == null)
                    return null;
                if (_webPSprites == null)
                {
                    _webPSprites = new List<Sprite>(frames.Count);
                    foreach (var frame in frames)
                        _webPSprites.Add(TextureHelper.GetSpriteByTexture(frame.texture));
                }
                return _webPSprites;
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
                    if (isGif)
                    {
                        // Static consumers (including dialogue illustration sizing)
                        // need the GIF canvas, not a 2x2 LoadImage placeholder.
                        var frames = GetGifFrames();
                        asset = frames != null && frames.Count > 0
                            ? frames[0].texture
                            : TextureHelper.transparentTexture;
                    }
                    else if (bytes != null)
                        asset = TextureHelper.GetTextureByByte(bytes);
                    else if (!string.IsNullOrEmpty(path))
                        asset = TextureHelper.GetTextureByPath(path);
                    else
                        asset = TextureHelper.transparentTexture;
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
                    var texture = GetTex();
                    if (texture != null)
                        _sprite = TextureHelper.GetSpriteByTexture(texture);
                }
                return _sprite;
            }
        }
    }
}
namespace Z_DataSystem
{
    /// <summary>Each asset plays once, holds its last frame for interval, then advances.</summary>
    public static class TextureAnimation
    {
        public readonly struct Sample
        {
            public readonly Texture texture;
            public readonly int index;
            public readonly bool animated;
            public readonly bool completed;
            public Sample(Texture texture, int index, bool animated, bool completed)
            {
                this.texture = texture;
                this.index = index;
                this.animated = animated;
                this.completed = completed;
            }
        }

        public static Sample GetSample(IReadOnlyList<int> ids, float interval, double seconds, bool loop = true)
            => GetSample(ids, 0, ids == null ? 0 : ids.Count, interval, seconds, loop);

        public static Sample GetSample(int id, float interval, double seconds, bool loop = true)
            => GetSample(null, id, 1, interval, seconds, loop);

        private static Sample GetSample(IReadOnlyList<int> ids, int singleId, int count,
            float interval, double seconds, bool loop)
        {
            double hold = interval > 0 && !float.IsInfinity(interval) ? interval : 0;
            double duration = 0;
            bool internalAnimation = false;
            for (int i = 0; i < count; i++)
            {
                double assetDuration = TexAssetForm.DataById.TryGetValue(ids == null ? singleId : ids[i], out var data)
                    ? data.GetAnimationDuration() : 0;
                duration += assetDuration + hold;
                internalAnimation |= assetDuration > 0;
            }
            bool animated = duration > 0 && (internalAnimation || count > 1);
            seconds = double.IsNaN(seconds) || double.IsInfinity(seconds) ? 0 : Math.Max(0, seconds);
            bool completed = !loop && duration > 0 && seconds >= duration;
            if (loop && duration > 0) seconds %= duration;
            for (int i = 0; i < count; i++)
            {
                TexAssetForm.DataById.TryGetValue(ids == null ? singleId : ids[i], out var data);
                double span = (data == null ? 0 : data.GetAnimationDuration()) + hold;
                if (duration <= 0 || seconds < span || i == count - 1)
                    return new Sample(data?.GetAnimationTexture(seconds), i, animated, completed);
                seconds -= span;
            }
            return new Sample(null, 0, false, false);
        }
    }

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
                AssetFilePicker.GetImage(path =>
                    OnImportComplete(string.IsNullOrEmpty(path) ? null : File.ReadAllBytes(path),
                        string.IsNullOrEmpty(path) ? null : Path.GetFileNameWithoutExtension(path)));
            }
            public override void OnImportComplete(byte[] data,string name)
            {
                var form = ctrl.CreateSelectedData(data, name, forceSize);
                if (form != null)
                    callback?.Invoke(form);
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
        public void SelectMultiple(Vector2Int forceSize = default, Action<IReadOnlyList<TexAssetForm.Data>> callback = null)
        {
            AssetFilePicker.GetImages(paths =>
            {
                var forms = new List<TexAssetForm.Data>();
                if (paths != null)
                {
                    foreach (var path in paths)
                    {
                        if (string.IsNullOrEmpty(path))
                            continue;
                        var form = CreateSelectedData(File.ReadAllBytes(path), Path.GetFileNameWithoutExtension(path), forceSize);
                        if (form != null)
                            forms.Add(form);
                    }
                }
                callback?.Invoke(forms);
            });
        }
        private TexAssetForm.Data CreateSelectedData(byte[] data, string name, Vector2Int forceSize)
        {
            if (data == null)
                return null;

            var tex = (Texture2D)TextureHelper.GetTextureByByte(data);
            if (tex == null)
                return null;
            // Validation creates a temporary texture; the asset itself stores the source bytes.
            UnityEngine.Object.Destroy(tex);
            var form = CreateDataByBytes(data, name);
            Z_EventHelper.Invoke(new AssetEvent()
            {
                importAssetName = name
            });
            return form;
        }
        public TexAssetForm.Data CreateDataByTex(Texture2D tex, string name, Vector2Int forceSize)
        {
            return CreateDataByBytes(TextureTransform.GetTargetSize(tex, forceSize.x, forceSize.y).EncodeToPNG(), name);
        }
        public TexAssetForm.Data CreateDataByTex(Texture2D tex, string name)
        {
            return new TexAssetForm.Data(-1, name, "", null, "", tex, 0);
        }
        public TexAssetForm.Data CreateDataByBytes(byte[] data, string name)
        {
            return new TexAssetForm.Data(-1, name, "", data, BytesSerialize.GetHash(data), null, 0);
        }
        public TexAssetForm.Data CreateDataByPath(string path, string name)
        {
            path = SaveAndLoad.GetRealPath(path);
            return new TexAssetForm.Data(-1, name, path, null, "", null, 0);
        }
        public TexAssetForm.Data CreateDataByTex(Texture tex, string name)
        {
            return CreateDataByTex((Texture2D)tex, name);
        }

    }
}
