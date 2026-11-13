using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using UnityEditor;
using UnityEngine;
using Z_Audio;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_UnitSystem;
using Picker = Ui.ModAssetSelectWindow;
using Object = UnityEngine.Object;

// Isolated project only. All generated files stay under this fixture's own root.
public static class EmptyMediaAssetsRegression
{
    private const BindingFlags HiddenInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly List<GameObject> owned = new List<GameObject>();
    private static AssetManager assets;
    private static GameSaveController save;
    private static int checks;

    public static void Run()
    {
        string previousRoot = SaveAndLoad.perPath;
        try
        {
            foreach (var type in new[] { typeof(AssetForm), typeof(TexAssetForm), typeof(StoryTexAssetForm),
                         typeof(AudioAssetForm), typeof(StoryAudioAssetForm), typeof(VideoAssetForm), typeof(StoryVideoAssetForm),
                         typeof(LabForm), typeof(BoxDataForm) })
            {
                type.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
                type.GetMethod("InitInternal", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
            }
            SaveAndLoad.perPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "EmptyMediaAssets");
            Directory.CreateDirectory(SaveAndLoad.perPath);
            assets = Manager<AssetManager>();
            assets.texCtrl = new TexController(assets);
            assets.audioCtrl = new AudioController(assets);
            assets.videoCtrl = new VideoController(assets);
            save = new GameSaveController(null);
            save.EnsureEmptyAssets();
            CreationAndReferences();
            PickerProtection();
            SilentPlayback();
            PersistenceAndReset();
            Debug.Log("EMPTY_MEDIA_ASSETS_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        finally
        {
            SaveAndLoad.perPath = previousRoot;
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        }
    }

    private static void CreationAndReferences()
    {
        var ids = new[] { GlobalDefaultHelper.DefaultTexId, GlobalDefaultHelper.DefaultAudioId, GlobalDefaultHelper.DefaultVideoId };
        Check(ids.Distinct().Count() == 3 && ids.All(id => id > AssetForm.autoIdCnt), "all empty assets use distinct preserved IDs in the shared Asset Form pool");
        var rows = ids.Select(id => AssetForm.DataById[id]).ToArray();
        Check(rows[0] is StoryTexAssetForm.Data && rows[1] is StoryAudioAssetForm.Data && rows[2] is StoryVideoAssetForm.Data,
            "initialization creates concrete selectable Story rows for all three kinds");
        Check(rows.All(row => row.name == string.Empty && row.labId == LabForm.NoneId && GlobalDefaultHelper.IsEmptyAsset(row)),
            "empty assets are unclassified and recognized by reserved identity");
        Check(rows.Skip(1).All(row => string.IsNullOrEmpty(row.path) && row.bytes == null && row.asset == null),
            "empty audio and video require no clip, payload or backing media path");
        int count = AssetForm.DataById.Count;
        save.EnsureEmptyAssets();
        Check(AssetForm.DataById.Count == count && rows.All(row => ReferenceEquals(AssetForm.DataById[row.id], row)),
            "repeated initialization preserves the same rows without consuming more IDs");
        Check(assets.texCtrl.GetName() == "$i$$i$" && assets.audioCtrl.GetName() == "$a$$a$" && assets.videoCtrl.GetName() == "$v$$v$",
            "all controllers produce the same empty-marker format");
        Check(assets.texCtrl.GetId("$i$$i$") == ids[0] && assets.audioCtrl.GetId("$a$$a$") == ids[1]
            && assets.videoCtrl.GetId("$v$$v$") == ids[2], "empty references resolve to their real reserved rows");
        Check(assets.audioCtrl.GetId(assets.audioCtrl.GetName(ids[1])) == ids[1]
            && assets.videoCtrl.GetId(assets.videoCtrl.GetName(ids[2])) == ids[2], "numeric references to the empty rows remain valid");
        Check(assets.audioCtrl.GetId(null) == -1 && assets.videoCtrl.GetId("junk$v$$v$") == -1
            && assets.texCtrl.GetId("$i$$i$junk") == -1, "malformed or absent references do not become empty assets");
    }

    private static void PickerProtection()
    {
        var resolve = typeof(Picker.UiModAssetSelectWindowCtrl).GetMethod("ResolveInitialSelection", BindingFlags.Static | BindingFlags.NonPublic);
        var replace = typeof(Picker.UiModAssetSelectWindowCtrl).GetMethod("ApplyReplacement", HiddenInstance);
        var rows = new AssetForm.Data[] { StoryTexAssetForm.DataById[GlobalDefaultHelper.DefaultTexId],
            StoryAudioAssetForm.DataById[GlobalDefaultHelper.DefaultAudioId], StoryVideoAssetForm.DataById[GlobalDefaultHelper.DefaultVideoId] };
        var parameters = new Picker.UiModAssetSelectWindowParam[] {
            new Picker.UiModAssetSelectTexWindowParam(), new Picker.UiModAssetSelectAudioWindowParam(), new Picker.UiModAssetSelectVideoWindowParam() };
        for (int i = 0; i < rows.Length; i++)
        {
            var row = rows[i]; parameters[i].selectedAssetId = row.id;
            Check(ReferenceEquals(resolve.Invoke(null, new object[] { parameters[i] }), row), "the picker can select the empty row in the matching media scope");
            var ctrl = new Picker.UiModAssetSelectWindowCtrl { model = new Picker.UiModAssetSelectWindowModel { sel = row } };
            Check(!ctrl.TryDeleteSelectedAsset() && ReferenceEquals(AssetForm.DataById[row.id], row), "the normal delete entry point refuses the empty row");
            row.name = "externally-renamed-default";
            Check(!ctrl.TryDeleteSelectedAsset(), "a changed display name cannot bypass reserved-ID deletion protection");
            row.name = string.Empty;
            var bytes = row.bytes; var path = row.path; var hash = row.hash;
            replace.Invoke(ctrl, new object[] { new AudioAssetForm.Data(-1, "replacement", "not-opened", new byte[] { 9 }, "changed", null, 0) });
            ctrl.Replace();
            Check(ReferenceEquals(row.bytes, bytes) && row.path == path && row.hash == hash,
                "empty rows cannot be replaced or open a native file picker");
        }
        var normal = new StoryAudioAssetForm.Data(-1, "ordinary-audio", string.Empty, null, string.Empty, null, 0);
        StoryAudioAssetForm.AddData(normal);
        var picker = new Picker.UiModAssetSelectWindowCtrl { model = new Picker.UiModAssetSelectWindowModel { sel = normal } };
        Check(picker.TryDeleteSelectedAsset() && !AssetForm.DataById.ContainsKey(normal.id)
            && !StoryAudioAssetForm.DataById.ContainsKey(normal.id), "ordinary assets retain their existing deletion behavior");
    }

    private static void SilentPlayback()
    {
        // No MediaPlayer pool is initialized: any attempted Open/Get would fail.
        var audioManager = Manager<AudioManager>();
        audioManager.Play(string.Empty);
        StoryAudioAssetForm.DataById[GlobalDefaultHelper.DefaultAudioId].Play(null);
        int completions = 0;
        var handler = StoryVideoAssetForm.DataById[GlobalDefaultHelper.DefaultVideoId].Play(null, () => completions++);
        Check(handler == null && completions == 1, "empty media neither opens a player nor waits for a playback event");
        var play = typeof(PlayAudioCmd).GetMethod("ExecuteInternal", HiddenInstance);
        Check((bool)play.Invoke(new PlayAudioCmd(), new object[] { new[] { CodeHelper.CreateBoxByStr("$a$$a$") }, null }),
            "PlayAudio accepts the empty reference without a missing-resource error");
        var show = typeof(ShowVideoCmd).GetMethod("ExecuteInternal", HiddenInstance);
        foreach (string reference in new[] { "$v$$v$", assets.videoCtrl.GetName(GlobalDefaultHelper.DefaultVideoId) })
            Check((bool)show.Invoke(new ShowVideoCmd(), new object[] { new[] { CodeHelper.CreateBoxByStr(reference) }, null }),
                "ShowVideo with an empty asset completes immediately without opening Dialog or pausing BGM");
    }

    private static void PersistenceAndReset()
    {
        var audio = new AudioAssetForm.Data(-1, "saved-audio", string.Empty, new byte[] { 1, 2, 3 }, string.Empty, null, 0);
        var video = new VideoAssetForm.Data(-1, "saved-video", string.Empty, new byte[] { 4, 5, 6 }, string.Empty, null, 0);
        save.AddStoryAudio(audio); save.AddStoryVideo(video);
        int audioId = StoryAudioAssetForm.DataById.Values.Single(row => row.name == audio.name).id;
        int videoId = StoryVideoAssetForm.DataById.Values.Single(row => row.name == video.name).id;
        save.SaveAssets("Core/");
        string folder = Path.Combine(SaveAndLoad.perPath, "Core", "ast");
        var files = Directory.GetFiles(folder).Select(Path.GetFileName).ToArray();
        Check(files.Length == 3 && files.Count(name => name.StartsWith("$a$")) == 1 && files.Count(name => name.StartsWith("$v$")) == 1,
            "only the image and real imported media have backing files; placeholders produce no fake audio/video files");
        save.LoadAsset("Core/");
        Check(StoryAudioAssetForm.DataById.ContainsKey(GlobalDefaultHelper.DefaultAudioId)
            && StoryVideoAssetForm.DataById.ContainsKey(GlobalDefaultHelper.DefaultVideoId), "asset manifests retain both empty Story rows");
        Check(string.IsNullOrEmpty(StoryAudioAssetForm.DataById[GlobalDefaultHelper.DefaultAudioId].path)
            && string.IsNullOrEmpty(StoryVideoAssetForm.DataById[GlobalDefaultHelper.DefaultVideoId].path),
            "reload does not manufacture missing file paths for empty media");
        Check(File.Exists(StoryAudioAssetForm.DataById[audioId].path) && File.Exists(StoryVideoAssetForm.DataById[videoId].path),
            "ordinary media still reloads its ID-and-hash backing files");
        save.SaveAssets("Core/");
        Check(Directory.GetFiles(folder).Length == 3, "repeated saving creates no duplicate or empty media files");
        save.LoadAsset(null);
        Check(!StoryAudioAssetForm.DataById.ContainsKey(audioId) && !StoryVideoAssetForm.DataById.ContainsKey(videoId)
            && StoryAudioAssetForm.DataById.ContainsKey(GlobalDefaultHelper.DefaultAudioId)
            && StoryVideoAssetForm.DataById.ContainsKey(GlobalDefaultHelper.DefaultVideoId), "Story reset removes imported media while preserving the reserved empty rows");
        save.EnsureEmptyAssets();
        Check(AssetForm.DataById.Count == 3, "initializing after a Story switch retains exactly one empty asset of each kind");
    }

    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var go = new GameObject(typeof(T).Name); owned.Add(go); go.SetActive(false); var result = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, result); return result;
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); checks++; }
}
