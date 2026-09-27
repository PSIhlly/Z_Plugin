using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;

// Run only in the isolated project created by Run-MapRuntimeRegression.ps1.
public static class MinimapRuntimeRegression
{
    private static int checks;

    public static void Run()
    {
        try
        {
            var map = CreateManager<MapManager>();
            map.data = new MapInfo
            {
                maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
                mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
            };
            var game = CreateManager<GameManager>();
            game.curScene = Form.SceneForm.GetDataByJo(new JObject());
            var play = CreateManager<PlayManager>();
            play.enable = false;
            int uid = 700000;
            for (int y = 0; y < 3; y++)
            for (int z = 2; z <= 3; z++)
            for (int x = -1; x <= 1; x++)
            {
                var tile = new TileUnitForm.Data(uid++, "", new Dictionary<int, int>(), new Vector3Int(x, y, z),
                    "", new Vector3(x, y, z), Vector3.zero, Vector3.one,
                    UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
                tile.unlock = x == -1 && y == 0 && z == 2;
                map.data.RegisterMap(tile);
            }
            var ctrl = new PlayMapController(play);
            ctrl.Begin();
            Check(ctrl.isReady && ctrl.cols == 3 && ctrl.rows == 2, "bounds initialize before Play updates start");
            var a = ctrl.unlockTextureMap[0];
            var b = ctrl.unlockTextureMap[1];
            var c = ctrl.unlockTextureMap[2];
            Check(a.mipmapCount == 1 && b.mipmapCount == 1 && c.mipmapCount == 1, "unlock masks have no mipmaps");
            Check(a.format == TextureFormat.RGBA32 && a.isReadable, "mask stays writable RGBA32");
            Check(a.GetPixel(0, 0) == Color.black && a.GetPixel(8, 0) == Color.clear, "saved unlock and transparent pixels initialize correctly");
            play.enable = true;
            Reveal(ctrl, map.data.maps[(0, 0, 2)]);
            Reveal(ctrl, map.data.maps[(1, 0, 3)]);
            Reveal(ctrl, map.data.maps[(-1, 1, 3)]);
            Reveal(ctrl, map.data.maps[(0, 0, 2)]);
            var dirtyLayers = (HashSet<int>)typeof(PlayMapController)
                .GetField("dirtyUnlockLayers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ctrl);
            Check(dirtyLayers.SetEquals(new[] { 0, 1 }), "reveals queue only the two changed heights");
            Check(a.GetPixel(8, 0) == Color.black && a.GetPixel(23, 15) == Color.black,
                "batch writes cover both corners of each 8x8 block");
            Check(a.GetPixel(16, 0) == Color.clear && b.GetPixel(0, 0) == Color.clear
                && b.GetPixel(0, 8) == Color.black, "neighbour pixels and bottom-to-top rows remain correct");
            Check(ctrl.unlockTiles.Count == 4, "duplicate reveals deduplicate with saved unlocks");
            Check(ctrl.FlushUnlockTextures() == 2 && dirtyLayers.Count == 0,
                "exactly two dirty heights upload and clear in one flush");
            Check(c.GetPixel(0, 0) == Color.clear, "unchanged height stays transparent");
            Check(ctrl.FlushUnlockTextures() == 0, "empty flush performs no uploads");
            Reveal(ctrl, map.data.maps[(0, 0, 2)]);
            Check(ctrl.FlushUnlockTextures() == 0, "repeat reveals do not upload");
            Reveal(ctrl, map.data.maps[(0, 1, 2)]);
            Check(ctrl.FlushUnlockTextures() == 1, "later changes upload only their own height");
            Reveal(ctrl, map.data.maps[(1, 1, 2)]);
            ctrl.End();
            Check(ctrl.FlushUnlockTextures() == 0 && !ctrl.isReady && dirtyLayers.Count == 0, "exit discards pending uploads");
            ctrl.Begin();
            var restored = ctrl.unlockTextureMap[1];
            Check(restored != b && restored.GetPixel(16, 0) == Color.black,
                "reentry reconstructs saved unlocks even if the last upload was discarded");
            Check(ctrl.FlushUnlockTextures() == 0, "reentry has no stale pending layers");
            ctrl.End();
            Debug.Log("MINIMAP_RUNTIME_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var go = new GameObject(typeof(T).Name);
        go.SetActive(false);
        var manager = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, manager);
        return manager;
    }

    private static void Reveal(PlayMapController ctrl, TileUnitForm.Data tile)
    {
        // TileUnit.Show persists unlock before dispatching the Show event.
        tile.unlock = true;
        ctrl.OnEvent(new TileEvent { type = MapEventType.Show, unit = tile.unit });
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
        checks++;
    }
}
