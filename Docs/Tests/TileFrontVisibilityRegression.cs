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
using Z_Time;
using Z_UnitSystem.Form;

// Isolated scene-renderer checks; never load the game scene or player saves.
public static class TileFrontVisibilityRegression
{
    private const string PrefabName = "MapPrefab$front-regression";
    private static int checks;

    public static void Run()
    {
        bool previousPlaying = DynamicGlobalSettings.playing;
        try
        {
            TexAssetForm.InitInternal();
            MapTextureForm.InitInternal();
            var map = CreateManager<MapManager>();
            map.data = new MapInfo { maps = new Dictionary<(int, int, int), TileUnitForm.Data>() };
            map.utilCtrl = new MapUtilController(map);
            var time = CreateManager<TimeManager>();
            time.gameObject.SetActive(true);
            var controller = new GameMapController(null);
            var texture = new Texture2D(2, 2);
            TexAssetForm.DataById[89101] = new TexAssetForm.Data(89101, "front", "", null, "", texture, 0);
            TexAssetForm.DataById[89102] = new TexAssetForm.Data(89102, "next", "", null, "", new Texture2D(2, 2), 0);
            var product = new MapTextureForm.Data(89103, "front-test", 89101, 0,
                new List<int> { 89101 }, 0, new Dictionary<string, EventTriggerForm.Data>(),
                false, new Dictionary<int, int>(), 0, true, new List<int> { 89101 },
                false, new Dictionary<int, int>());
            MapTextureForm.DataById[product.id] = product;
            var shader = Shader.Find("DisFadeCode");
            Check(shader != null, "real DisFadeCode shader is available");
            var source = new Material(shader);
            source.SetFloat("_Alpha", 1f);
            source.SetFloat("_Show", 1f);
            var root = new GameObject("tile-front-regression");
            for (int i = 0; i < 6; i++)
            {
                var child = new GameObject("layer" + i);
                child.transform.SetParent(root.transform, false);
                child.AddComponent<MeshRenderer>().sharedMaterial = source;
            }
            var instance = root.AddComponent<TileInstance>();
            var tile = NewTile(89104, new Vector3Int(10, 2, 20), product.id);
            Bind(instance, tile);
            var show = new TileEvent { type = MapEventType.Show, unit = tile.unit };
            DynamicGlobalSettings.playing = true;
            controller.OnEvent(show);
            CheckLayers(instance, true, "open edge");

            var rear = NewTile(89105, tile.mapPos + Vector3Int.back, product.id);
            rear.texDic.Clear(); // Classification uses prefab names, not appearance or a live instance.
            var rearKey = (rear.mapPos.x, rear.mapPos.y, rear.mapPos.z);
            map.data.maps[rearKey] = rear;
            Check(rear.unit.ins == null, "rear tile need not be shown");
            controller.OnEvent(show);
            CheckLayers(instance, false, "same prefab directly behind");
            instance.ApplyVision(.5f, 1f);
            instance.VisOff();
            instance.VisOn();
            CheckLayers(instance, false, "vision updates retain front suppression");

            rear.prefabName = PrefabName + "-other";
            controller.OnEvent(show);
            CheckLayers(instance, true, "different prefab");
            rear.prefabName = PrefabName.ToUpperInvariant();
            controller.OnEvent(show);
            CheckLayers(instance, true, "prefab names match ordinally");
            rear.prefabName = PrefabName;
            map.data.maps.Remove(rearKey);
            map.data.maps[(rear.mapPos.x, rear.mapPos.y - 1, rear.mapPos.z)] = rear;
            controller.OnEvent(show);
            CheckLayers(instance, true, "lower neighbour is not the same layer");
            map.data.maps.Clear();
            map.data.maps[(tile.mapPos.x, tile.mapPos.y, tile.mapPos.z + 1)] = rear;
            map.data.maps[(tile.mapPos.x + 1, tile.mapPos.y, tile.mapPos.z)] = rear;
            controller.OnEvent(show);
            CheckLayers(instance, true, "forward and side neighbours do not hide the rear front");
            map.data.maps.Clear();
            map.data.maps[rearKey] = rear;
            DynamicGlobalSettings.playing = false;
            controller.OnEvent(show);
            CheckLayers(instance, true, "Mod keeps authored front parts visible");

            map.data.maps.Clear();
            DynamicGlobalSettings.playing = true;
            product.frontPartTexs.Add(89102);
            product.animTimeInterval = .5f;
            controller.OnEvent(show);
            var previousTimers = (Timer[])instance.animTimer.Clone();
            for (int i = 3; i < 6; i++)
                Check(previousTimers[i] != null && !previousTimers[i].cancel, "open front animation starts " + i);
            map.data.maps[rearKey] = rear;
            controller.OnEvent(show);
            CheckLayers(instance, false, "covered animated fronts");
            for (int i = 3; i < 6; i++)
                Check(previousTimers[i].cancel && instance.animTimer[i] == null, "hidden front animation is canceled " + i);

            // A pool handoff changes owners on the same renderer hierarchy.
            instance.PrepareForHide();
            instance.PrepareForShow();
            var next = NewTile(89106, new Vector3Int(30, 2, 40), product.id);
            Bind(instance, next);
            controller.OnEvent(new TileEvent { type = MapEventType.Show, unit = next.unit });
            CheckLayers(instance, true, "next pooled owner has no rear neighbour");
            for (int i = 3; i < 6; i++)
                Check(instance.animTimer[i] != null && !instance.animTimer[i].cancel, "next owner's front animation restarts " + i);

            Debug.Log("TILE_FRONT_VISIBILITY_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally { DynamicGlobalSettings.playing = previousPlaying; }
    }

    private static TileUnitForm.Data NewTile(int uid, Vector3Int position, int productId)
    {
        return new TileUnitForm.Data(uid, "front-test", new Dictionary<int, int>
            { [0] = productId, [1] = productId, [2] = productId }, position,
            PrefabName, position, Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), "", false, false, new List<int>());
    }

    private static void Bind(TileInstance instance, TileUnitForm.Data tile)
    {
        if (instance.unit != null) instance.unit.ins = null;
        instance.unit = tile.unit;
        tile.unit.ins = instance;
        instance.ApplyVision(1f); // Unit.Show applies vision before the Tile Show event.
    }

    private static void CheckLayers(TileInstance instance, bool showFront, string scenario)
    {
        for (int i = 0; i < 3; i++)
        {
            Check(instance.renderers[i].enabled, scenario + ": body " + i);
            Check(instance.renderers[i + 3].enabled == showFront, scenario + ": front " + i);
        }
    }

    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject(typeof(T).Name);
        root.SetActive(false);
        var manager = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, manager);
        return manager;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
