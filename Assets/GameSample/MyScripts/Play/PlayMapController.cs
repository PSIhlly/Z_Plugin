using Form;
using System;
using System.Collections.Generic;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;

public class PlayMapController : Z_Controller<PlayManager>, IZ_Listener<ObjectEvent>, IZ_Listener<ItemEvent>, IZ_Listener<CharacterEvent>, IZ_Listener<TileEvent>
{
    public int tileSize = 8;
    public Dictionary<int, (Vector3, Sprite)> dic;

    public SceneForm.Data curScene => GameManager.instance.curScene;
    public bool isReady { get; private set; }
    Dictionary<(int, int, int), TileUnitForm.Data> maps => MapManager.instance.data.maps;
    public Dictionary<int, Sprite> heightMap;
    public Dictionary<int, Texture2D> unlockTextureMap;
    public HashSet<int> unlockTiles;
    private readonly HashSet<int> dirtyUnlockLayers = new HashSet<int>();
    private Color[] unlockedTilePixels;
    public (int, int, int, int) size;
    public int rows => size.Item1 - size.Item2 + 1;
    public int cols => size.Item4 - size.Item3 + 1;
    public PlayMapController(PlayManager super) : base(super)
    {
        dic = new Dictionary<int, (Vector3, Sprite)>();
        heightMap = new Dictionary<int, Sprite>();
        unlockTextureMap = new Dictionary<int, Texture2D>();
        unlockTiles = new HashSet<int>();
    }

    public void Begin()
    {
        isReady = false;
        dirtyUnlockLayers.Clear();
        heightMap.Clear();
        unlockTiles.Clear();
        unlockTextureMap.Clear();
        dic.Clear();
        this.Register<ObjectEvent>();
        this.Register<ItemEvent>();
        this.Register<CharacterEvent>();
        this.Register<TileEvent>();
        RegisterNewScene();
        GenerateMinimapFromTiles();
        curScene.unlock = true;
        isReady = rows > 0 && cols > 0;
    }
    public void End()
    {
        isReady = false;
        dirtyUnlockLayers.Clear();
        this.Unregister<ObjectEvent>();
        this.Unregister<ItemEvent>();
        this.Unregister<CharacterEvent>();
        this.Unregister<TileEvent>();
    }

    private void Manage(MapEventType tp, MapUnit unit, int icon)
    {
        if (!_super.enable || unit.belongTile == null || !unit.belongTile.data.unlock || icon <= 0 || icon == GlobalDefaultHelper.ExternDefaultTexId)
            return;
        switch (tp)
        {
            case MapEventType.Move:
            case MapEventType.Create:
                dic[unit.data.uid] = (unit.data.pos, StoryTexAssetForm.DataById.GetDk(icon, GlobalDefaultHelper.ExternDefaultTexId).GetSprite());
                break;
            case MapEventType.Hide:
                dic.Remove(unit.data.uid);
                break;
        }
    }
    public bool HasMinimap()
    {
        if (curScene == null)
            return false;
        var map = StoryTexAssetForm.DataById.GetDv(curScene.miniMap, null);
        return map != null && curScene.miniMap != GlobalDefaultHelper.ExternDefaultTexId;
    }
    public void RegisterNewScene()
    {
        // Scene setup runs before PlayManager enables its frame updates.
        // An empty map has zero rows/columns, rather than retaining the previous scene bounds.
        size = (0, 1, 1, 0);
        if (maps == null || maps.Count == 0)
            return;

        size.Item4 = int.MinValue;
        size.Item3 = int.MaxValue;
        size.Item2 = int.MaxValue;
        size.Item1 = int.MinValue;

        foreach (var kvp in maps)
        {
            var pos = kvp.Value.mapPos;
            size.Item2 = Math.Min(size.Item2, pos.z);
            size.Item1 = Math.Max(size.Item1, pos.z);
            size.Item3 = Math.Min(size.Item3, pos.x);
            size.Item4 = Math.Max(size.Item4, pos.x);
        }

    }
    public void GenerateMinimapFromTiles()
    {
        dirtyUnlockLayers.Clear();
        if (maps == null || rows <= 0 || cols <= 0)
            return;
        EnsureUnlockPixelBlock();
        // Reuse one transparent buffer for all height masks during scene setup.
        var clearPixels = new Color32[cols * tileSize * rows * tileSize];
        Dictionary<int, List<TileUnitForm.Data>> dic = new Dictionary<int, List<TileUnitForm.Data>>();
        foreach (var kvp in maps)
        {
            if (!dic.ContainsKey(kvp.Value.mapPos.y))
            {
                dic[kvp.Value.mapPos.y] = new List<TileUnitForm.Data>();
            }

            dic[kvp.Value.mapPos.y].Add(kvp.Value);
        }
        foreach (var pair in dic)
        {
            var unlock = new Texture2D(cols * tileSize, rows * tileSize, TextureFormat.RGBA32, false);
            unlock.SetPixels32(clearPixels);
            Texture2D[] tileTextures = new Texture2D[rows * cols];
            var tileDatas = pair.Value;
            foreach (var tileData in tileDatas)
            {
                int col = tileData.mapPos.x - size.Item3;
                // Texture rows run bottom-to-top, matching increasing map Z and WangTile UVs.
                int row = tileData.mapPos.z - size.Item2;

                var tileTexture = GameMapController.GetTileLayerTexture(tileData, 0) as Texture2D;
                if (tileTexture != null)
                    tileTextures[row * cols + col] = TextureTransform.GetTargetSize(tileTexture, tileSize, tileSize);

                if (tileData.unlock)
                {
                    unlock.SetPixels(col * tileSize, row * tileSize, tileSize, tileSize, unlockedTilePixels);
                    unlockTiles.Add(tileData.uid);
                }
            }
            Texture2D minimapTex = TextureCombine.FillTexture2DsToTexture2D(tileTextures, cols, rows, 1, (Vector2Int.one * tileSize));
            minimapTex.wrapMode = TextureWrapMode.Clamp;
            unlock.wrapMode = TextureWrapMode.Clamp;
            Sprite minimapSprite = TextureHelper.GetSpriteByTexture(minimapTex);
            unlock.Apply(false, false);
            heightMap[pair.Key] = minimapSprite;
            unlockTextureMap[pair.Key] = unlock;
        }

    }
    public bool TryGetRelativePosition(Vector3 worldPosition, out Vector2 relativePosition)
    {
        relativePosition = default;
        if (!isReady || cols <= 0 || rows <= 0)
            return false;

        var mapPosition = MapManager.instance.utilCtrl.RealPos2MapPos(worldPosition);
        // Bounds describe cell centers; include the half-cell border on both sides.
        relativePosition = new Vector2(
            (mapPosition.x - size.Item3 + 0.5f) / cols,
            (mapPosition.z - size.Item2 + 0.5f) / rows);
        return !float.IsNaN(relativePosition.x) && !float.IsInfinity(relativePosition.x)
            && !float.IsNaN(relativePosition.y) && !float.IsInfinity(relativePosition.y);
    }

    public void UpdateUnlock(TileUnitForm.Data data)
    {
        if (!_super.enable || !isReady || data == null)
            return;
        int col = data.mapPos.x - size.Item3;
        int row = data.mapPos.z - size.Item2;
        if (col < 0 || col >= cols || row < 0 || row >= rows)
            return;
        if (unlockTextureMap.TryGetValue(data.mapPos.y, out Texture2D unlock) && unlockTiles.Add(data.uid))
        {
            EnsureUnlockPixelBlock();
            unlock.SetPixels(col * tileSize, row * tileSize, tileSize, tileSize, unlockedTilePixels);
            dirtyUnlockLayers.Add(data.mapPos.y);
        }
    }

    private void EnsureUnlockPixelBlock()
    {
        int pixelCount = tileSize * tileSize;
        if (unlockedTilePixels != null && unlockedTilePixels.Length == pixelCount)
            return;
        unlockedTilePixels = new Color[pixelCount];
        for (int i = 0; i < pixelCount; i++)
            unlockedTilePixels[i] = Color.black;
    }

    public int FlushUnlockTextures()
    {
        if (!isReady || dirtyUnlockLayers.Count == 0)
            return 0;
        int uploads = 0;
        // Called once in Play LateUpdate, after movement and event-driven reveals.
        foreach (int layer in dirtyUnlockLayers)
            if (unlockTextureMap.TryGetValue(layer, out var unlock))
            {
                unlock.Apply(false, false);
                uploads++;
            }
        dirtyUnlockLayers.Clear();
        return uploads;
    }

    public void OnEvent(CharacterEvent evt)
    {
        Manage(evt.type, evt.unit, evt.unit.data.minimapIcon);
    }

    public void OnEvent(ItemEvent evt)
    {
        Manage(evt.type, evt.unit, evt.unit.data.minimapIcon);
    }

    public void OnEvent(ObjectEvent evt)
    {
        Manage(evt.type, evt.unit, evt.unit.data.minimapIcon);
    }

    public void OnEvent(TileEvent evt)
    {
        if (evt.type == MapEventType.Show)
        {
            UpdateUnlock(evt.unit.data);
        }
    }
}
