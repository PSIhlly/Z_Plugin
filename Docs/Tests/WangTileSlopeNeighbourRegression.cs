using System;
using System.Collections.Generic;
using System.Reflection;
using Form;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

// Isolated data-selection checks. Never open the game scene or player saves.
public static class WangTileSlopeNeighbourRegression
{
    private const int TextureProduct = 89301;
    private const BindingFlags StaticPrivate = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly MethodInfo MaskMethod = typeof(GameMapController).GetMethod("GetWangTileMask", StaticPrivate);
    private static readonly MethodInfo FramesMethod = typeof(GameMapController).GetMethod("GetTileLayerTextures", StaticPrivate);
    private static readonly MethodInfo MinimapMethod = typeof(GameMapController).GetMethod("GetTileLayerTexture", StaticPrivate);
    private static readonly (int x, int z)[] Neighbours =
    {
        (-1, 1), (0, 1), (1, 1), (-1, 0), (1, 0), (-1, -1), (0, -1), (1, -1)
    };
    private static int checks;
    private static int nextUid = 89320;
    private static MapManager map;

    public static void Run()
    {
        bool previousPlaying = DynamicGlobalSettings.playing;
        CameraMode previousCameraMode = DynamicGlobalSettings.cameraMode;
        try
        {
            Check(MaskMethod != null && FramesMethod != null && MinimapMethod != null,
                "production mask, base/front and minimap entry points are available");
            map = CreateManager<MapManager>();
            map.data = new MapInfo { maps = new Dictionary<(int, int, int), TileUnitForm.Data>() };
            DynamicGlobalSettings.playing = true;
            DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
            CheckLowerSlopeMasks();
            CheckMatchingAndPriority();
            CheckUpperSlopeMasks();
            CheckCoordinateBoundaries();
            CheckSharedSamplers();
            CheckHistoryRefresh();
            Debug.Log("WANG_TILE_SLOPE_NEIGHBOUR_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            DynamicGlobalSettings.playing = previousPlaying;
            DynamicGlobalSettings.cameraMode = previousCameraMode;
        }
    }

    private static void CheckLowerSlopeMasks()
    {
        var center = NewTile(new Vector3Int(10, 3, 20));
        ResetMap(center);
        Check(Mask(center) == 0, "an isolated Tile has no neighbours");
        for (int bit = 0; bit < Neighbours.Length; bit++)
        for (int layer = 0; layer < 3; layer++)
        {
            ResetMap(center);
            var offset = Neighbours[bit];
            var slope = NewTile(center.mapPos + new Vector3Int(offset.x, -1, offset.z), "mapslope", layer);
            Add(slope);
            Check(Mask(center, layer) == 1 << bit, "lower slope supplies bit " + bit + " at logical texture layer " + layer);
        }
        for (int expected = 0; expected < 256; expected++)
        {
            ResetMap(center);
            for (int bit = 0; bit < Neighbours.Length; bit++)
                if ((expected & (1 << bit)) != 0)
                {
                    var offset = Neighbours[bit];
                    Add(NewTile(center.mapPos + new Vector3Int(offset.x, -1, offset.z), "mapslope"));
                }
            Check(Mask(center) == expected, "all eight independent lower-slope bits combine: " + expected);
        }
        for (int yaw = 0; yaw < 360; yaw += 90)
        {
            ResetMap(center);
            var slope = NewTile(center.mapPos + new Vector3Int(1, -1, 0), "mapslope");
            slope.euler = new Vector3(0, yaw, 0);
            Add(slope);
            Check(Mask(center) == 1 << 4, "lower-slope fallback is independent of the slope yaw " + yaw);
        }
    }

    private static void CheckMatchingAndPriority()
    {
        var center = NewTile(new Vector3Int(10, 3, 20));
        for (int bit = 0; bit < Neighbours.Length; bit++)
        {
            var offset = Neighbours[bit];
            Vector3Int adjacent = center.mapPos + new Vector3Int(offset.x, 0, offset.z);
            foreach (string prefab in new[] { "mapground", "mapfloor", "mapslope-other", "MAPSLOPE" })
            {
                ResetMap(center);
                Add(NewTile(adjacent + Vector3Int.down, prefab));
                Check(Mask(center) == 0, "only the canonical lower slope connects, not " + prefab + " bit " + bit);
            }
            ResetMap(center);
            Add(NewTile(adjacent + Vector3Int.down * 2, "mapslope"));
            Check(Mask(center) == 0, "fallback never searches two layers below, bit " + bit);
            Add(NewTile(adjacent + Vector3Int.down, "mapground"));
            Check(Mask(center) == 0, "a nearer non-slope cannot expose a deeper slope, bit " + bit);

            ResetMap(center);
            var lower = NewTile(adjacent + Vector3Int.down, "mapslope");
            Add(lower);
            lower.texDic[0] = TextureProduct + 1;
            Check(Mask(center) == 0, "lower slope must have the same MapTexture ID, bit " + bit);
            lower.texDic = new Dictionary<int, int> { [1] = TextureProduct };
            Check(Mask(center) == 0, "another logical layer is not a texture match, bit " + bit);
            Check(Mask(center, 1) == 1 << bit, "the same logical layer does match, bit " + bit);
            lower.texDic.Clear();
            Check(Mask(center) == 0, "a missing texture key stays disconnected, bit " + bit);
            lower.texDic = null;
            Check(Mask(center) == 0, "null lower-slope texture dictionary stays disconnected, bit " + bit);

            lower.texDic = new Dictionary<int, int> { [0] = TextureProduct };
            var same = NewTile(adjacent);
            Add(same);
            Check(Mask(center) == 1 << bit, "same-height ordinary Tile still connects, bit " + bit);
            same.texDic[0] = TextureProduct + 1;
            Check(Mask(center) == 0, "existing same-height Tile blocks lower fallback even when its ID differs, bit " + bit);
            same.texDic.Clear();
            Check(Mask(center) == 0, "existing same-height Tile with a missing key blocks fallback, bit " + bit);
            same.texDic = null;
            Check(Mask(center) == 0, "existing same-height Tile with null texture dictionary blocks fallback, bit " + bit);
            map.data.maps.Remove(Key(adjacent));
            Check(Mask(center) == 1 << bit, "removing the same-height Tile immediately restores lower fallback, bit " + bit);
            lower.prefabName = MapInfo.GetPrefabName("mapfloor");
            Check(Mask(center) == 0, "replacing the slope immediately removes its connection, bit " + bit);
            lower.prefabName = MapInfo.GetPrefabName("mapslope");
            Check(Mask(center) == 1 << bit, "restoring the slope immediately restores its connection, bit " + bit);
            map.data.maps.Remove(Key(lower.mapPos));
            Check(Mask(center) == 0, "removing a lower slope immediately removes its connection, bit " + bit);
        }
    }

    private static void CheckCoordinateBoundaries()
    {
        foreach (Vector3Int pos in new[] { Vector3Int.zero, new Vector3Int(-20, -3, -40), new Vector3Int(496, 500, 519) })
        {
            var center = NewTile(pos);
            ResetMap(center);
            for (int bit = 0; bit < Neighbours.Length; bit++)
            {
                var offset = Neighbours[bit];
                Add(NewTile(pos + new Vector3Int(offset.x, -1, offset.z), "mapslope"));
            }
            Check(Mask(center) == 255, "exact key lookup works at zero/negative/reported-world coordinates " + pos);
        }
    }

    private static void CheckUpperSlopeMasks()
    {
        var cases = new[]
        {
            (yaw: 0f, x: 0, z: 1), (yaw: 90f, x: 1, z: 0),
            (yaw: 180f, x: 0, z: -1), (yaw: 270f, x: -1, z: 0),
            (yaw: -90f, x: -1, z: 0), (yaw: 360f, x: 0, z: 1),
            (yaw: 450f, x: 1, z: 0), (yaw: -360f, x: 0, z: 1)
        };
        foreach (var facing in cases)
        {
            var center = NewTile(new Vector3Int(10, 3, 20), "mapslope");
            center.euler = new Vector3(0, facing.yaw, 0);
            for (int layer = 0; layer < 3; layer++)
            {
                ResetMap(center);
                int expected = 0;
                for (int bit = 0; bit < Neighbours.Length; bit++)
                {
                    var offset = Neighbours[bit];
                    Add(NewTile(center.mapPos + new Vector3Int(offset.x, 1, offset.z), "mapground", layer));
                    if (offset.x * facing.x + offset.z * facing.z > 0) expected |= 1 << bit;
                }
                Check(Mask(center, layer) == expected,
                    "self slope sees only uphill cardinal and two diagonals, yaw=" + facing.yaw + " layer=" + layer);
            }
            ResetMap(center);
            for (int bit = 0; bit < Neighbours.Length; bit++)
            {
                var offset = Neighbours[bit];
                bool uphill = offset.x * facing.x + offset.z * facing.z > 0;
                Vector3Int adjacent = center.mapPos + new Vector3Int(offset.x, 0, offset.z);
                ResetMap(center);
                var upper = NewTile(adjacent + Vector3Int.up, "mapfloor");
                Add(upper);
                Check(Mask(center) == (uphill ? 1 << bit : 0),
                    "an upper non-slope Tile connects exactly on the uphill side, yaw=" + facing.yaw + " bit=" + bit);
                upper.texDic[0] = TextureProduct + 1;
                Check(Mask(center) == 0, "uphill upper neighbour still requires identical texture ID");
                upper.texDic = new Dictionary<int, int> { [1] = TextureProduct };
                Check(Mask(center) == 0, "uphill upper neighbour still requires identical logical texture layer");
                upper.texDic = null;
                Check(Mask(center) == 0, "null upper-neighbour texture dictionary cannot connect");
                upper.texDic = new Dictionary<int, int> { [0] = TextureProduct };

                var same = NewTile(adjacent);
                same.texDic[0] = TextureProduct + 1;
                Add(same);
                Check(Mask(center) == (uphill ? 1 << bit : 0),
                    "uphill upper connection is additional even when same-height texture mismatches");
                same.texDic[0] = TextureProduct;
                upper.texDic[0] = TextureProduct + 1;
                Check(Mask(center) == 1 << bit,
                    "a mismatching upper Tile never suppresses an ordinary same-height match");
                map.data.maps.Remove(Key(adjacent));
                map.data.maps.Remove(Key(upper.mapPos));
                Add(NewTile(adjacent + Vector3Int.up * 2));
                Check(Mask(center) == 0, "self slope never searches two layers above");
            }
        }
        foreach (string prefab in new[] { "mapground", "mapfloor", "mapslope-other", "MAPSLOPE" })
        {
            var center = NewTile(new Vector3Int(10, 3, 20), prefab);
            ResetMap(center);
            foreach (var offset in Neighbours)
                Add(NewTile(center.mapPos + new Vector3Int(offset.x, 1, offset.z)));
            Check(Mask(center) == 0, "only a canonical self slope can connect upwards, not " + prefab);
        }
    }

    private static void CheckSharedSamplers()
    {
        MapTextureForm.InitInternal();
        TexAssetForm.InitInternal();
        var baseVariants = new Dictionary<int, int>();
        var frontVariants = new Dictionary<int, int>();
        for (int mask = 0; mask < 256; mask++)
        {
            baseVariants[mask] = 90000 + mask;
            frontVariants[mask] = 91000 + mask;
        }
        var product = new MapTextureForm.Data(TextureProduct, "slope WangTile regression", 90000, .25f,
            new List<int> { 92000 }, 0, new Dictionary<string, EventTriggerForm.Data>(), true, baseVariants,
            0, true, new List<int> { 92001 }, true, frontVariants);
        MapTextureForm.DataById[TextureProduct] = product;
        var center = NewTile(new Vector3Int(10, 3, 20));
        center.texDic[1] = TextureProduct;
        center.texDic[2] = TextureProduct;
        ResetMap(center);
        foreach (int bit in new[] { 0, 1, 4, 7 })
        {
            var offset = Neighbours[bit];
            var lower = NewTile(center.mapPos + new Vector3Int(offset.x, -1, offset.z), "mapslope");
            lower.texDic[1] = TextureProduct;
            lower.texDic[2] = TextureProduct;
            Add(lower);
        }
        const int expectedMask = (1 << 0) | (1 << 1) | (1 << 4) | (1 << 7);
        for (int layer = 0; layer < 3; layer++)
        {
            Check(SelectedFrame(center, layer, false) == baseVariants[expectedMask],
                "base WangTile sampler shares the lower-slope mask, layer " + layer);
            Check(SelectedFrame(center, layer, true) == frontVariants[expectedMask],
                "front WangTile sampler shares the lower-slope mask, layer " + layer);
        }
        var texture = new Texture2D(2, 2);
        TexAssetForm.DataById[baseVariants[expectedMask]] =
            new TexAssetForm.Data(baseVariants[expectedMask], "slope selected variant", "", null, "", texture, 0);
        Check(ReferenceEquals(MinimapMethod.Invoke(null, new object[] { center, 0 }), texture),
            "minimap samples the exact same lower-slope base WangTile variant without a visible instance");
        UnityEngine.Object.DestroyImmediate(texture);

        center.prefabName = MapInfo.GetPrefabName("mapslope");
        ResetMap(center);
        // Three uphill neighbours, one lower-slope fallback and one same-layer Tile combine.
        foreach (int bit in new[] { 0, 1, 2, 3, 4 })
        {
            var offset = Neighbours[bit];
            int dy = bit <= 2 ? 1 : (bit == 3 ? -1 : 0);
            var neighbour = NewTile(center.mapPos + new Vector3Int(offset.x, dy, offset.z),
                bit == 3 ? "mapslope" : "mapground");
            neighbour.texDic[1] = TextureProduct;
            neighbour.texDic[2] = TextureProduct;
            Add(neighbour);
        }
        const int mixedMask = 31;
        for (int layer = 0; layer < 3; layer++)
        {
            Check(SelectedFrame(center, layer, false) == baseVariants[mixedMask],
                "base sampler combines uphill, same-layer and lower-slope neighbours, layer " + layer);
            Check(SelectedFrame(center, layer, true) == frontVariants[mixedMask],
                "front sampler combines uphill, same-layer and lower-slope neighbours, layer " + layer);
        }
        texture = new Texture2D(2, 2);
        TexAssetForm.DataById[baseVariants[mixedMask]] =
            new TexAssetForm.Data(baseVariants[mixedMask], "uphill selected variant", "", null, "", texture, 0);
        Check(ReferenceEquals(MinimapMethod.Invoke(null, new object[] { center, 0 }), texture),
            "minimap shares the mixed uphill/same-layer/lower-slope WangTile mask");
        UnityEngine.Object.DestroyImmediate(texture);
        product.isWangTile = false;
        product.frontIsWangTile = false;
        Check(SelectedFrame(center, 0, false) == 92000 && SelectedFrame(center, 0, true) == 92001,
            "non-Wang base/front clips retain their ordinary texture selection");
    }

    private static void CheckHistoryRefresh()
    {
        map.data.maps.Clear();
        var updates = new MapUpdateController(map);
        map.updateCtrl = updates;
        typeof(MapUpdateController).GetField("hasView", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(updates, true);
        SetView(updates, (-100, 100, -100, 100, -100, 100));
        var pos = new Vector3Int(10, 3, 20);
        var probes = new Dictionary<Vector3Int, RefreshProbeTile>();
        for (int y = pos.y - 2; y <= pos.y + 2; y++)
        for (int x = pos.x - 2; x <= pos.x + 3; x++)
        for (int z = pos.z - 2; z <= pos.z + 2; z++)
        {
            var tile = NewTile(new Vector3Int(x, y, z));
            var probe = new RefreshProbeTile(tile);
            typeof(UnitForm.Data).GetField("_unit", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(tile, probe);
            probes[tile.mapPos] = probe;
            Add(tile);
        }
        updates.RefreshHistoryTiles(new[] { pos });
        CheckProbeVisits(probes, new[] { pos }, "one changed Tile");
        ClearProbes(probes);
        var overlapping = new[] { pos, pos, pos + Vector3Int.right, pos + Vector3Int.up };
        updates.RefreshHistoryTiles(overlapping);
        CheckProbeVisits(probes, overlapping, "duplicate and overlapping changed Tiles");
        Check(probes[pos + Vector3Int.up * 2].shows == 1,
            "a directly changed upper layer refreshes its own adjacent upper layer once");

        ClearProbes(probes);
        updates.RefreshHistoryTiles(Array.Empty<Vector3Int>());
        foreach (var probe in probes.Values)
            Check(probe.shows == 0 && probe.hides == 0, "an empty refresh batch does no work");

        ClearProbes(probes);
        updates.curTileLst.Clear();
        foreach (var probe in probes.Values) updates.curTileLst.Add(probe.data);
        SetView(updates, (-100, 100, pos.y, pos.y + 1, -100, 100));
        updates.RefreshHistoryTiles(new[] { pos });
        foreach (var pair in probes)
        {
            bool horizontal = Mathf.Abs(pair.Key.x - pos.x) <= 1 && Mathf.Abs(pair.Key.z - pos.z) <= 1;
            bool affected = horizontal && Mathf.Abs(pair.Key.y - pos.y) <= 1;
            bool inView = pair.Key.y == pos.y;
            Check(pair.Value.shows == (affected && inView ? 1 : 0)
                && pair.Value.hides == (affected && !inView ? 1 : 0),
                "refresh respects view bounds for lower/same/upper layers at " + pair.Key);
            Check(updates.curTileLst.Contains(pair.Value.data) == (!affected || inView),
                "only affected out-of-view Tiles leave the visible set at " + pair.Key);
        }

        ClearProbes(probes);
        SetView(updates, (-100, 100, -100, 100, -100, 100));
        map.data.maps.Remove(Key(pos));
        updates.RefreshHistoryTiles(new[] { pos });
        Check(probes[pos + Vector3Int.up + Vector3Int.right].shows == 1,
            "removing a lower Tile still refreshes the upper diagonal neighbour");
        Check(probes[pos + Vector3Int.down + Vector3Int.left].shows == 1,
            "removing an upper Tile still refreshes a lower slope neighbour");
        Check(probes[pos + Vector3Int.up * 2].shows == 0 && probes[pos + Vector3Int.down * 2].shows == 0,
            "an individual edit never visits Tiles separated by two layers");
    }

    private static void CheckProbeVisits(Dictionary<Vector3Int, RefreshProbeTile> probes,
        IEnumerable<Vector3Int> edited, string scenario)
    {
        var expected = new HashSet<Vector3Int>();
        foreach (Vector3Int pos in edited)
        for (int y = -1; y <= 1; y++)
        for (int x = -1; x <= 1; x++)
        for (int z = -1; z <= 1; z++)
            expected.Add(pos + new Vector3Int(x, y, z));
        foreach (var pair in probes)
            Check(pair.Value.shows == (expected.Contains(pair.Key) ? 1 : 0) && pair.Value.hides == 0,
                scenario + ": bounded local refresh is deduplicated at " + pair.Key);
    }

    private static void ClearProbes(Dictionary<Vector3Int, RefreshProbeTile> probes)
    {
        foreach (var probe in probes.Values) { probe.shows = 0; probe.hides = 0; }
    }

    private static void SetView(MapUpdateController controller, (int, int, int, int, int, int) value)
    {
        typeof(MapUpdateController).GetProperty("lastView").SetValue(controller, value);
    }

    private static int SelectedFrame(TileUnitForm.Data tile, int layer, bool front)
    {
        object result = FramesMethod.Invoke(null, new object[] { tile, layer, front });
        object frames = result.GetType().GetField("Item1").GetValue(result);
        int count = (int)frames.GetType().GetProperty("Count").GetValue(frames);
        Check(count == 1, "selected WangTile sequence contains exactly one configured frame");
        return (int)frames.GetType().GetProperty("Item").GetValue(frames, new object[] { 0 });
    }

    private static int Mask(TileUnitForm.Data tile, int layer = 0) =>
        (int)MaskMethod.Invoke(null, new object[] { tile, layer, TextureProduct });

    private static TileUnitForm.Data NewTile(Vector3Int position, string prefab = "mapground", int layer = 0) =>
        new TileUnitForm.Data(nextUid++, "slope neighbour regression", new Dictionary<int, int> { [layer] = TextureProduct },
            position, MapInfo.GetPrefabName(prefab), position, Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());

    private static void ResetMap(TileUnitForm.Data center) { map.data.maps.Clear(); Add(center); }
    private static (int, int, int) Key(Vector3Int pos) => (pos.x, pos.y, pos.z);
    private static void Add(TileUnitForm.Data tile) => map.data.maps[Key(tile.mapPos)] = tile;

    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject(typeof(T).Name);
        root.SetActive(false);
        var manager = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, manager);
        return manager;
    }

    private sealed class RefreshProbeTile : TileUnit
    {
        public int shows;
        public int hides;
        public RefreshProbeTile(TileUnitForm.Data data) : base(data) { }
        public override void Show() { shows++; }
        public override bool Hide() { hides++; return true; }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
