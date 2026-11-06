using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Form;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Texture;

// Run only in the isolated temporary Unity project created by the regression runner.
public static class WangTileStoryCacheRegression
{
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private static int checks;

    public static void Run()
    {
        try
        {
            TexAssetForm.InitInternal();
            MapTextureForm.InitInternal();
            var ctrl = new GameMapController(null);
            ctrl.Reset();
            WangTileStoryCache(ctrl);
            Debug.Log("WANG_TILE_STORY_CACHE_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    private static void WangTileStoryCache(GameMapController ctrl)
    {
        LabForm.InitInternal();
        GameTexAssetForm.InitInternal();
        ObjectProductForm.InitInternal();
        MapTextureForm.Clear();
        ObjectProductForm.Clear();
        var game = CreateManager<GameManager>();
        game.saveCtrl = new GameSaveController(game);
        var manager = CreateManager<Main2StoryManager>();
        var red = new Texture2D(4, 6);
        var blue = new Texture2D(4, 6);
        var source = new TexAssetForm.Data(89901, "story-wang-source", "", null, "", red, 0);
        TexAssetForm.DataById[source.id] = source;
        var tile = new MapTextureForm.Data(89902, "story-wang", source.id, 0,
            new List<int> { source.id }, 0, new Dictionary<string, EventTriggerForm.Data>(), true,
            new Dictionary<int, int>(), 0, false, new List<int> { source.id }, true, new Dictionary<int, int>());
        MapTextureForm.AddData(tile);
        var ensure = typeof(Main2StoryManager).GetMethod("EnsureWangTileTextures", PrivateInstance);
        var reusable = typeof(Main2StoryManager).GetMethod("CanReuseWangTileTextures", PrivateInstance);
        var release = typeof(Main2StoryManager).GetMethod("DestroyWangTileTextures", PrivateInstance);
        var animations = (IDictionary)typeof(GameMapController).GetField("wangTileAnimationTextures", PrivateStatic).GetValue(null);
        Action generate = () => ensure.Invoke(manager, null);
        Func<bool> canReuse = () => (bool)reusable.Invoke(manager, null);
        try
        {
            Check(!canReuse(), "a new Story has no reusable WangTile generation");
            generate();
            int firstId = tile.WangTileDic[0];
            var firstTexture = GameTexAssetForm.DataById[firstId].asset;
            Check(tile.WangTileDic.Count == 256 && animations.Count == 256 && canReuse(),
                "first entry builds variants and captures the resolved source");
            generate();
            Check(tile.WangTileDic[0] == firstId && ReferenceEquals(firstTexture, GameTexAssetForm.DataById[firstId].asset),
                "another scene reuses generated IDs and Texture objects");
            ctrl.Reset();
            Check(animations.Count == 256 && canReuse(), "scene reset retains the Story animation registrations");

            tile.enableFrontPart = true;
            Check(!canReuse(), "enabling WangTile front invalidates the generation");
            generate();
            Check(tile.FrontWangTileDic.Count == 256 && animations.Count == 512 && canReuse(),
                "changed front configuration rebuilds both variant registrations");
            source.ClearRuntimeCache();
            Check(!canReuse(), "same-ID asset replacement invalidates a resolved source");
            source.asset = blue;
            generate();
            Check(canReuse(), "replacement source becomes reusable after regeneration");
            source.SetRuntimeAnimationFrames(new[] { new AnimatedFrameData { texture = blue, delaySeconds = .1f } });
            Check(!canReuse(), "new internal animation frames invalidate even an unchanged first Texture");
            generate();
            Check(canReuse(), "generated animated variants are reusable too");
            int oldId = tile.WangTileDic[0];
            release.Invoke(manager, null);
            Check(!canReuse() && !GameTexAssetForm.DataById.ContainsKey(oldId)
                && tile.WangTileDic.Count == 0 && tile.FrontWangTileDic.Count == 0 && animations.Count == 0,
                "Story release removes generated assets, compatibility maps, source state and animations");
            generate();
            Check(canReuse() && tile.WangTileDic[0] != oldId, "a subsequent Story lifetime builds fresh generated IDs");
        }
        finally
        {
            release.Invoke(manager, null);
            MapTextureForm.RemoveData(tile.id);
            TexAssetForm.DataById.Remove(source.id);
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
            UnityEngine.Object.DestroyImmediate(game.gameObject);
            UnityEngine.Object.DestroyImmediate(red);
            UnityEngine.Object.DestroyImmediate(blue);
        }
    }

    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var go = new GameObject(typeof(T).Name);
        go.SetActive(false);
        var manager = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, manager);
        return manager;
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
        checks++;
    }
}

