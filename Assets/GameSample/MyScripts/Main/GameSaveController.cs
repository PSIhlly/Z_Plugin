using Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RenderHeads.Media.AVProVideo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Z_ByteSerialize;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_UnitSystem;

public class GameSaveController : Z_Controller<GameManager>
{
    public string mapTextureFormFileName => "mtf";
    public string mapMaskFormFileName => "mmf";
    public string mapObjectFormFileName => "mof";
    public string objectProductFormFileName => "oprf";
    public string storyFormFileName => "sf";
    public string characterParamFormFileName => "cpaf";
    public string skillParamFormFileName => "spaf";

    public string mapObjectParamFormFileName => "opaf";
    public string sceneFormFileName => "saf";
    public string characterProductFormFileName => "cprf";
    public string eventFormFileName => "ef";
    public string itemParamFormFileName => "ipaf";
    public string effectFormFileName => "etf";
    public string skillProductFormFileName => "splf";
    public string itemProductFormFileName => "iprf";
    public string imageUiItemFormFileName => "iuif";

    public string progressFormFileName => "pf";
    public string imageAssetFormFileName => "iaff";
    public string audioAssetFormFileName => "aaff";
    public string videoAssetFormFileName => "vaff";
    public string labFormFileName => "lf";
    public string missionFormFileName => "msf";
    public string passTypeFormFileName => "ptf";

    public string assetFolder => "ast/";
    private JArray legacyObjectForms;
    public GameSaveController(GameManager super) : base(super)
    {
    }

    #region package

    public byte[] Package(int storyId)
    {
        Texture2D icon;
        try
        { 
            icon = (Texture2D)AssetManager.instance.texCtrl.CreateDataByBytes(Convert.FromBase64String(GameManager.instance.curStory.icon), "tmp").GetTex();
        }
        catch (Exception e)
        {
            icon = Texture2D.whiteTexture;
        }
        return TextureHelper.GetPNGWithExtraInfo(icon, System.Text.Encoding.UTF8.GetBytes(SaveAndLoad.Package(Main2StoryManager.GetStoryCoreFolder(storyId))));
    }

    // Enter on the Unity thread: capture the cover and absolute path before any
    // worker touches files. No Texture, Form, or Unity API is used by the worker.
    public Task<byte[]> PackageAsync(int storyId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cover = CapturePackageCover(storyId);
        var core = SaveAndLoad.GetRealPath(Main2StoryManager.GetStoryCoreFolder(storyId));
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var png = PngPixelPackage.EncodeStreamWithCover(cover,
                output => SaveAndLoad.WritePackage(core, output, cancellationToken), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return png;
        }, cancellationToken);
    }

    private static PngPixelPackage.CoverSnapshot CapturePackageCover(int storyId)
    {
        Texture2D ownedTexture = null;
        List<GifFrameData> ownedFrames = null;
        Texture2D icon = Texture2D.whiteTexture;
        try
        {
            try
            {
                var bytes = Convert.FromBase64String(StoryForm.DataById[storyId].icon);
                if (bytes.Length >= 3 && bytes[0] == 'G' && bytes[1] == 'I' && bytes[2] == 'F')
                {
                    ownedFrames = TextureHelper.GetGifFramesByByte(bytes);
                    icon = ownedFrames != null && ownedFrames.Count > 0
                        ? ownedFrames[0].texture : TextureHelper.transparentTexture;
                }
                else
                    icon = ownedTexture = (Texture2D)TextureHelper.GetTextureByByte(bytes);
            }
            catch
            {
                icon = Texture2D.whiteTexture;
            }
            return PngPixelPackage.CaptureCover(icon);
        }
        finally
        {
            // Only our private decoded cover/frames are owned here. Shared
            // white/transparent fallback textures and registered assets survive.
            if (ownedFrames != null)
                foreach (var frame in ownedFrames)
                    DestroyPackageCover(frame.texture);
            DestroyPackageCover(ownedTexture);
        }
    }

    private static void DestroyPackageCover(Texture2D texture)
    {
        if (texture == null)
            return;
        if (Application.isPlaying)
            UnityEngine.Object.Destroy(texture);
        else
            UnityEngine.Object.DestroyImmediate(texture);
    }

    #endregion

    #region unpackage

    public void Unpackage(byte[] content, int storyId)
    {
        SaveAndLoad.Unpackage(ReadPackage(content).ToString(), Main2StoryManager.GetStoryCoreFolder(storyId));
    }

    // New .png exports store data in RGB pixels; legacy ZEXTRA PNGs remain
    // readable. Register one overview, never reload/clear global Forms.
    public StoryForm.Data ImportPackage(byte[] content)
    {
        var files = ReadPackage(content);
        var overview = ReadPackageOverview(files);
        var story = StoryForm.GetDataByJo(overview);
        var id = 1;
        for (; id < StoryForm.autoIdCnt; id++)
            if (!StoryForm.DataById.ContainsKey(id) && !SaveAndLoad.Exist(Main2StoryManager.GetStoryFolderNameById(id)))
                break;
        if (id >= StoryForm.autoIdCnt)
            throw new InvalidOperationException("No free local story ID is available.");
        story.id = id;
        overview["id"] = id;

        // Stage away from numeric story directories so failed imports are never
        // discovered as stories. Publish the complete Core in one directory move.
        var stageName = ".mod-import-" + Guid.NewGuid().ToString("N");
        var stageRoot = SaveAndLoad.GetRealPath(stageName);
        var stageCore = stageName + "/Core/";
        var targetRoot = SaveAndLoad.GetRealPath(Main2StoryManager.GetStoryFolderNameById(id));
        var moved = false;
        try
        {
            Directory.CreateDirectory(stageRoot);
            SaveAndLoad.Unpackage(files.ToString(), stageCore);
            SaveAndLoad.Save(stageCore + storyFormFileName, overview.ToString());
            Directory.Move(stageRoot, targetRoot); // Does not overwrite a raced/existing directory.
            moved = true;
            StoryForm.AddData(story);
            return story;
        }
        catch
        {
            if (StoryForm.DataById.TryGetValue(id, out var registered) && ReferenceEquals(registered, story))
                StoryForm.RemoveData(id);
            // These exact roots were newly created by this request, never an
            // existing story. Do not delete targetRoot when the move failed.
            var ownedRoot = moved ? targetRoot : stageRoot;
            if (Directory.Exists(ownedRoot))
                Directory.Delete(ownedRoot, true);
            throw;
        }
    }

    public async Task<StoryForm.Data> ImportPackageAsync(string path, CancellationToken cancellationToken = default)
    {
        // Force SaveAndLoad's Unity-backed static initialization on the caller
        // thread and keep workers independent of later perPath changes.
        var root = SaveAndLoad.GetRealPath(".");
        var prepared = await Task.Run(() => PreparePackageImport(path, root, cancellationToken), cancellationToken);
        StoryForm.Data story = null;
        string targetRoot = null;
        var moved = false;
        var committed = false;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Generated Form parsing, IDs and registration belong to Unity's
            // thread, never to the decoder/extraction worker.
            story = StoryForm.GetDataByJo(prepared.overview);
            var id = 1;
            for (; id < StoryForm.autoIdCnt; id++)
            {
                targetRoot = Path.Combine(root, Main2StoryManager.GetStoryFolderNameById(id));
                if (!StoryForm.DataById.ContainsKey(id) && !Directory.Exists(targetRoot) && !File.Exists(targetRoot))
                    break;
            }
            if (id >= StoryForm.autoIdCnt)
                throw new InvalidOperationException("No free local story ID is available.");
            story.id = id;
            prepared.overview["id"] = id;
            var overviewContent = prepared.overview.ToString();
            await Task.Run(() => File.WriteAllText(Path.Combine(prepared.stageRoot, "Core", storyFormFileName),
                overviewContent), cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            if (StoryForm.DataById.ContainsKey(id))
                throw new InvalidOperationException("The local story ID became unavailable. Please retry importing.");
            // Directory.Move rejects an existing/raced directory. Commit without
            // an await/cancellation point between publishing Core and its row.
            Directory.Move(prepared.stageRoot, targetRoot);
            moved = true;
            StoryForm.AddData(story);
            committed = true;
            return story;
        }
        catch
        {
            if (story != null && StoryForm.DataById.TryGetValue(story.id, out var registered) && ReferenceEquals(registered, story))
                StoryForm.RemoveData(story.id);
            throw;
        }
        finally
        {
            if (!committed)
            {
                var ownedRoot = moved ? targetRoot : prepared.stageRoot;
                // Wait for every writer before cleanup, even when canceled.
                await Task.Run(() => DeleteImportStage(ownedRoot));
            }
        }
    }

    private sealed class PreparedPackageImport
    {
        public string stageRoot;
        public JObject overview;
    }

    private PreparedPackageImport PreparePackageImport(string path, string root, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var files = ReadPackage(File.ReadAllBytes(path));
        var overview = ReadPackageOverview(files);
        cancellationToken.ThrowIfCancellationRequested();
        var stageRoot = Path.Combine(root, ".mod-import-" + Guid.NewGuid().ToString("N"));
        try
        {
            var core = Path.Combine(stageRoot, "Core");
            Directory.CreateDirectory(core);
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                SaveAndLoad.Save(Path.Combine(core, file.Key), (byte[])file.Value);
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new PreparedPackageImport { stageRoot = stageRoot, overview = overview };
        }
        catch
        {
            DeleteImportStage(stageRoot);
            throw;
        }
    }

    private JObject ReadPackageOverview(JObject files)
    {
        var overviewBytes = (byte[])files[storyFormFileName];
        if (overviewBytes == null)
            throw new InvalidDataException("The Mod package has no story overview (sf).");
        var overview = JObject.Parse(new System.Text.UTF8Encoding(false, true).GetString(overviewBytes));
        if (overview["name"]?.Type != JTokenType.String ||
            (overview["icon"] != null && overview["icon"].Type != JTokenType.String))
            throw new InvalidDataException("Invalid story overview. Legacy cover data requires migration.");
        return overview;
    }

    private static void DeleteImportStage(string ownedRoot)
    {
        if (Directory.Exists(ownedRoot))
            Directory.Delete(ownedRoot, true);
    }

    private static JObject ReadPackage(byte[] content)
    {
        var signature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        if (content == null || content.Length < 20 || !signature.SequenceEqual(content.Take(8)))
            throw new InvalidDataException("Not a valid Mod package.");
        // Check chunk bounds before extraction. Old ZEXTRA exports precede
        // IHDR; new pixel packages use standard PNG chunk order.
        var offset = 8;
        var ended = false;
        while (offset <= content.Length - 12)
        {
            uint length = ((uint)content[offset] << 24) | ((uint)content[offset + 1] << 16) |
                          ((uint)content[offset + 2] << 8) | content[offset + 3];
            if (length > content.Length - offset - 12)
                throw new InvalidDataException("Truncated Mod package.");
            var type = System.Text.Encoding.ASCII.GetString(content, offset + 4, 4);
            offset += (int)length + 12;
            if (type == "IEND")
            {
                ended = length == 0 && offset == content.Length;
                break;
            }
        }
        if (!ended)
            throw new InvalidDataException("Incomplete Mod package.");
        var extra = TextureHelper.GetExtraInfoByPNG(content);
        if (extra == null)
            throw new InvalidDataException("This file contains no Mod package data.");
        var root = JObject.Parse(new System.Text.UTF8Encoding(false, true).GetString(extra));
        var files = new JObject();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<JObject>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            foreach (var entry in queue.Dequeue())
            {
                // Existing Package emits /directory objects plus flat file keys.
                if (entry.Key.StartsWith("/") && entry.Value is JObject directory)
                {
                    queue.Enqueue(directory);
                    continue;
                }
                var path = entry.Key.Replace('\\', '/');
                if (string.IsNullOrEmpty(path) || path.Split('/').Any(part =>
                    string.IsNullOrEmpty(part) || part == "." || part == ".." ||
                    part.EndsWith(".") || part.EndsWith(" ") ||
                    part.Any(c => c < ' ' || "<>:\"|?*".IndexOf(c) >= 0)) ||
                    entry.Value?.Type != JTokenType.String || !names.Add(path))
                    throw new InvalidDataException("Invalid or duplicate file path in Mod package: " + entry.Key);
                // Validate Base64 for every file before any extraction/write.
                files[path] = (byte[])entry.Value;
            }
        }
        return files;
    }

    #endregion

    #region save
    public void SaveSceneMap(string scenePath)
    {
        if (MapManager.instance.data != null)
        {
            SaveAndLoad.Save(scenePath, JsonConvert.SerializeObject(MapManager.instance.data.GetJsonData()));
        }
    }
    public void SaveSceneMap(string scenePath, MapInfo data)
    {
        SaveAndLoad.Save(scenePath, JsonConvert.SerializeObject(data.GetJsonData()));
    }
    public void SaveSaveStory(int id)
    {
        string path = Main2StoryManager.GetStorySaveFolder(id);

        ObjectProductForm.EnsurePrototypes(); // Create concrete-owned labels before saving lf.
        SaveLab(path);
        SavePassType(path);
        SaveMaterial(path);
        SaveObject(path);
        SaveObjectProducts(path, true);
        SaveCharacter(path);
        SaveSkill(path);
        SaveItem(path);
        SaveEffect(path);
        SaveMission(path);
        SaveEvent(path, null);
        SaveConfig(path);
        SaveScene(path);

        SaveUiItem(path);
        SaveProgress(path);

        SaveSceneMap(PlayManager.instance.GetSceneCacheFileName());

        CopyMapScene(Main2StoryManager.GetStoryCacheFolder(id), path);
    }
    public void SaveCoreStory(int id)
    {
        SaveOverview(id);

        string path = Main2StoryManager.GetStoryCoreFolder(id);

        ObjectProductForm.EnsurePrototypes();
        SaveLab(path);
        SavePassType(path);
        SaveMaterial(path);
        SaveObject(path);
        SaveObjectProducts(path, false);
        SaveCharacter(path);
        SaveSkill(path);
        SaveItem(path);
        SaveEffect(path);
        SaveMission(path);
        SaveEvent(path);
        SaveConfig(path);
        SaveScene(path);

        foreach (var data in SceneForm.DataByUid.Values)
        {
            if (!SaveAndLoad.Exist(path + Main2StoryManager.GetSceneFileNameById(data.uid)))
            {
                var mapData = new GameMapData();
                var dic = GameManager.instance.innerAssetDic;
                mapData.Init((GameObjectAssetForm.Data)dic["mapground"], (GameObjectAssetForm.Data)dic["mapslope"], (GameObjectAssetForm.Data)dic["mapfloor"], (GameObjectAssetForm.Data)dic["img"], (GameObjectAssetForm.Data)dic["canvas"],(TexAssetForm.Data) dic["defaultTileTexture"]);
                GameManager.instance.saveCtrl.SaveSceneMap(path + Main2StoryManager.GetSceneFileNameById(data.uid), mapData);
            }
        }

        SaveAndLoad.Delete(Main2StoryManager.GetStorySaveFolder(id));
        SaveAssets(path);
    }
    public void SaveOverview(int id)
    {
        var storyCoreFolder = Main2StoryManager.GetStoryCoreFolder(id);
        SaveAndLoad.Save(storyCoreFolder + "/" + storyFormFileName, StoryForm.GetJoByData(StoryForm.DataById[id]).ToString());

    }

    public void SaveLab(string storyFolder)
    {
        // CmdLab is built-in command metadata, not story-authored labels.
        CmdLabForm.Init();
        var labs = new JArray();
        foreach (var lab in LabForm.DataById.Values)
            if (!(lab is CmdLabForm.Data) && lab.belong != nameof(GameCmdDataForm))
                labs.Add(LabForm.GetJoByData(lab));
        SaveAndLoad.Save(storyFolder + "/" + labFormFileName, labs.ToString());
    }

    public void SavePassType(string storyFolder)
    {
        RepairPassTypeReferences();
        SaveAndLoad.Save(storyFolder + "/" + passTypeFormFileName, PassTypeForm.GetJaByDatas().ToString());
    }

    public void SaveMaterial(string storyCoreFolder)
    {

        SaveAndLoad.Save(storyCoreFolder + "/" + mapTextureFormFileName, MapTextureForm.GetJaByDatas().ToString());

        SaveAndLoad.Save(storyCoreFolder + "/" + mapMaskFormFileName, MapMaskForm.GetJaByDatas().ToString());


    }
    public void SaveObject(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + mapObjectParamFormFileName, MapObjectParamForm.GetJaByDatas().ToString());
    }

    public void SaveObjectProducts(string folder, bool includeRuntime)
    {
        ObjectProductForm.EnsurePrototypes();
        // An explicit authored save also upgrades unvisited source scenes
        // before the obsolete MapObject table can be retired.
        if (!includeRuntime)
            ObjectProductForm.PrepareModStory(folder);
        var products = new JArray();
        foreach (var product in ObjectProductForm.DataByUid.Values)
            if (includeRuntime || product.protoUid == 0)
            {
                product.EnsureDirectionData();
                product.SyncLegacyAnimClip(product.GetDefaultAnimDirection());
                products.Add(ObjectProductForm.GetJoByData(product));
            }
        SaveAndLoad.Save(folder + "/" + objectProductFormFileName, products.ToString());
        // Delete only this obsolete table after the replacement is saved.
        if (SaveAndLoad.Exist(folder + "/" + mapObjectFormFileName))
            SaveAndLoad.Delete(folder + "/" + mapObjectFormFileName);
    }

    public void SaveCharacter(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + characterParamFormFileName, CharacterParamForm.GetJaByDatas().ToString());

        foreach (var data in CharacterProductForm.DataByUid.Values)
        {
            if (data.size < 1)
                data.size = 1;
        }
        SaveAndLoad.Save(storyCoreFolder + "/" + characterProductFormFileName, CharacterProductForm.GetJaByDatas().ToString());

    }
    public void SaveSkill(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + skillParamFormFileName, SkillParamForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + skillProductFormFileName, SkillProductForm.GetJaByDatas().ToString());

    }
    public void SaveItem(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + itemParamFormFileName, ItemParamForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + itemProductFormFileName, ItemProductForm.GetJaByDatas().ToString());
    }
    public void SaveEffect(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + effectFormFileName, EffectForm.GetJaByDatas().ToString());

    }

    public void SaveMission(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + missionFormFileName, MissionForm.GetJaByDatas().ToString());
    }

    public void SaveEvent(string storyCoreFolder, EventProgramDataForm.Data data = null)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + eventFormFileName, EventProgramDataForm.GetJaByDatas().ToString());



    }

    public void SaveConfig(string storyFolder)
    {
        SaveAndLoad.Save(storyFolder + "/" + progressFormFileName, ProgressForm.GetJaByDatas().ToString());
    }

    public void SaveProgress(string storyFolder)
    {
        SaveAndLoad.Save(storyFolder + "/" + progressFormFileName, ProgressForm.GetJaByDatas().ToString());
    }
    public void SaveUiItem(string storySaveFolder)
    {
        SaveAndLoad.Save(storySaveFolder + "/" + imageUiItemFormFileName, ImageUiItemForm.GetJaByDatas().ToString());
    }
    public void SaveScene(string storyCoreFolder)
    {
        SaveAndLoad.Save(storyCoreFolder + "/" + sceneFormFileName, SceneForm.GetJaByDatas().ToString());

    }
    public void SaveAssets(string storyCoreFolder)
    {
        EnsureStoryAssetHashes();

        SaveAndLoad.Save(storyCoreFolder + "/" + imageAssetFormFileName, StoryTexAssetForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + audioAssetFormFileName, StoryAudioAssetForm.GetJaByDatas().ToString());
        SaveAndLoad.Save(storyCoreFolder + "/" + videoAssetFormFileName, StoryVideoAssetForm.GetJaByDatas().ToString());

        var expectedAssetNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var data in StoryTexAssetForm.DataById.Values)
        {
            var name = SaveStoryTex(data, storyCoreFolder);
            if (name != null)
                expectedAssetNames.Add(name);
        }
        foreach (var data in StoryAudioAssetForm.DataById.Values)
        {
            var name = SaveStoryAudio(data, storyCoreFolder);
            if (name != null)
                expectedAssetNames.Add(name);
        }
        foreach (var data in StoryVideoAssetForm.DataById.Values)
        {
            var name = SaveStoryVideo(data, storyCoreFolder);
            if (name != null)
                expectedAssetNames.Add(name);
        }
        SaveAndLoad.EachFile(storyCoreFolder + assetFolder, (name) =>
        {
            if (!expectedAssetNames.Contains(name))
                SaveAndLoad.Delete(storyCoreFolder + assetFolder + name);
        });
    }
    #region util
    private void SaveTex(int texId, string path)
    {
        var data = TexAssetForm.DataById.GetDv(texId, null);
        if (data != null && data.bytes != null && !GlobalDefaultHelper.IsInnerAssetName(texId))
        {

            path = path + assetFolder + texId;
            if (!SaveAndLoad.Exist(path))
            {
                SaveAndLoad.Save(path, data.bytes);
            }

        }
    }
    private string SaveStoryTex(StoryTexAssetForm.Data data, string path)
    {
        if (data == null || GlobalDefaultHelper.IsInnerAssetName(data.id))
            return null;

        var name = GetStoryAssetFileName(AssetManager.instance.texCtrl, data);
        path = path + assetFolder + name;
        if (!SaveAndLoad.Exist(path))
        {
            var bytes = GetAssetBytes(data);
            if (bytes != null)
                SaveAndLoad.Save(path, bytes);
        }
        if (SaveAndLoad.Exist(path))
            data.path = SaveAndLoad.GetRealPath(path);
        return name;
    }
    private string SaveStoryVideo(StoryVideoAssetForm.Data data, string path)
    {
        if (data == null || GlobalDefaultHelper.IsInnerAssetName(data.id))
            return null;

        var name = GetStoryAssetFileName(AssetManager.instance.videoCtrl, data);
        path = path + assetFolder + name;
        if (!SaveAndLoad.Exist(path))
        {
            if (data.bytes != null)
                SaveAndLoad.Save(path, data.bytes);
            else if (!string.IsNullOrEmpty(data.path))
                SaveAndLoad.Copy(data.path, path);
        }
        if (SaveAndLoad.Exist(path))
            data.path = SaveAndLoad.GetRealPath(path);
        return name;
    }
    private string SaveStoryAudio(StoryAudioAssetForm.Data data, string path)
    {
        if (data == null || GlobalDefaultHelper.IsInnerAssetName(data.id))
            return null;

        var name = GetStoryAssetFileName(AssetManager.instance.audioCtrl, data);
        path = path + assetFolder + name;
        if (!SaveAndLoad.Exist(path))
        {
            if (data.bytes != null)
                SaveAndLoad.Save(path, data.bytes);
            else if (!string.IsNullOrEmpty(data.path))
                SaveAndLoad.Copy(data.path, path);
        }
        if (SaveAndLoad.Exist(path))
            data.path = SaveAndLoad.GetRealPath(path);
        return name;
    }
    private void EnsureStoryAssetHashes()
    {
        foreach (var data in StoryTexAssetForm.DataById.Values)
            EnsureAssetHash(data);
        foreach (var data in StoryAudioAssetForm.DataById.Values)
            EnsureAssetHash(data);
        foreach (var data in StoryVideoAssetForm.DataById.Values)
            EnsureAssetHash(data);
    }
    private static void EnsureAssetHash(AssetForm.Data data)
    {
        if (data == null || GlobalDefaultHelper.IsInnerAssetName(data.id))
            return;

        var bytes = GetAssetBytes(data);
        if (bytes == null)
            return;

        var contentHash = BytesSerialize.GetHash(bytes);
        if (!string.Equals(data.hash, contentHash, StringComparison.Ordinal))
            data.hash = contentHash;
    }
    private static byte[] GetAssetBytes(AssetForm.Data data)
    {
        if (data.bytes != null)
            return data.bytes;
        if (string.IsNullOrEmpty(data.path))
            return null;

        var path = Path.IsPathRooted(data.path) ? data.path : SaveAndLoad.GetRealPath(data.path);
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }
    private static string GetStoryAssetFileName(IAssetController controller, AssetForm.Data data)
    {
        if (string.IsNullOrEmpty(data.hash))
            return controller.GetName(data.id);
        return $"{controller.GetMark()}{data.id}${data.hash}{controller.GetMark()}";
    }
    private string GetStoryAssetPath(string folder, IAssetController controller, AssetForm.Data data)
    {
        var hashPath = folder + assetFolder + GetStoryAssetFileName(controller, data);
        if (!string.IsNullOrEmpty(data.hash) && SaveAndLoad.Exist(hashPath))
            return SaveAndLoad.GetRealPath(hashPath);

        var legacyPath = folder + assetFolder + controller.GetName(data.id);
        if (SaveAndLoad.Exist(legacyPath))
            return SaveAndLoad.GetRealPath(legacyPath);
        return SaveAndLoad.GetRealPath(hashPath);
    }
    #endregion
    #endregion

    #region load
    public void LoadCoreStory(int id)
    {
        ResetStory();
        var folder = Main2StoryManager.GetStoryCoreFolder(id);
        var assetFolder = Main2StoryManager.GetStoryAssetFolder(id);
        LoadLab(folder);
        LoadPassType(folder);
        LoadScene(folder);
        LoadMaterial(folder);
        LoadObject(folder);
        LoadCharacter(folder);
        RepairPassTypeReferences();
        LoadSkill(folder);

        LoadItem(folder);
        LoadObjectProducts(folder);
        LoadEffect(folder);
        LoadMission(folder);

        LoadEvent(folder);
        LoadProgress(folder);

        DeleteCache(id);
        CopyMapScene(folder, Main2StoryManager.GetStoryCacheFolder(id));
        ObjectProductForm.PrepareModStory(Main2StoryManager.GetStoryCacheFolder(id));
        LoadAsset(assetFolder);
        RepairMissingCharacterTextureReferences();
    }
    public void ResetStory()
    {
        LoadObjectProducts(null);
        LoadScene(null);
        LoadMaterial(null);
        LoadObject(null);
        LoadCharacter(null);
        LoadSkill(null);

        LoadItem(null);
        LoadEffect(null);
        LoadMission(null);

        LoadEvent(null);
        LoadProgress(null);
        LoadUiItem(null);
        LoadAsset(null);
        LoadPassType(null);
        LoadLab(null);
    }
    public void LoadSaveStory(int id)
    {
        ResetStory();
        var folder = Main2StoryManager.GetStorySaveFolder(id);

        var assetFolder = Main2StoryManager.GetStoryAssetFolder(id);
        LoadLab(folder);
        LoadPassType(folder);
        LoadScene(folder);
        LoadMaterial(folder);
        LoadObject(folder);
        LoadCharacter(folder);
        RepairPassTypeReferences();
        LoadSkill(folder);

        LoadItem(folder);
        LoadObjectProducts(folder);
        LoadEffect(folder);
        LoadMission(folder);

        LoadEvent(folder);
        LoadProgress(folder);
        if (RepairSavedCurrentCharacter())
            SaveProgress(folder);
        //manage Scene
        DeleteCache(id);
        if (LackMapScene(folder))
        {
            CopyMapScene(Main2StoryManager.GetStoryCoreFolder(id), folder);
        }
        CopyMapScene(folder, Main2StoryManager.GetStoryCacheFolder(id));
        //
        LoadUiItem(folder);
        LoadAsset(assetFolder);
        RepairMissingCharacterTextureReferences();

    }

    public void LoadOverview()
    {
        StoryForm.Clear();
        string[] allDirectories = Directory.GetDirectories(SaveAndLoad.perPath);
        foreach (string dir in allDirectories)
        {
            var coreFolder = Main2StoryManager.GetStoryCoreFolder(dir);
            if (int.TryParse(Path.GetFileName(dir), out int id) && SaveAndLoad.Exist(coreFolder + storyFormFileName))
            {
                var form = StoryForm.GetDataByJo(JObject.Parse(SaveAndLoad.Load<string>(coreFolder + storyFormFileName)));
                form.id = id;//矫正
                StoryForm.AddData(form);
            }
        }
    }

    public void LoadLab(string folder)
    {
        CmdLabForm.Init();
        LabForm.ClearAuto();
        if (folder == null)
            return;

        var pathForm = folder + labFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in LabForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                // Older lf files included static command labels as base Lab rows.
                if (form.belong == nameof(GameCmdDataForm) || CmdLabForm.DataById.ContainsKey(form.id))
                    continue;
                LabForm.AddData(form);
            }
        }
    }

    public void LoadPassType(string folder)
    {
        PassTypeForm.ClearAuto();
        if (folder == null)
            return;

        var pathForm = folder + passTypeFormFileName;
        if (!SaveAndLoad.Exist(pathForm))
            return;

        foreach (var form in PassTypeForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            PassTypeForm.AddData(form);
    }

    private static void MigrateFlatLabIds(JArray forms, string belong, params string[] legacyKeys)
    {
        foreach (JObject form in forms)
        {
            if (TryNormalizeCurrentLabId(form, belong))
                continue;

            form["labId"] = LabForm.NoneId;
            foreach (var legacyKey in legacyKeys)
            {
                if (!form.TryGetValue(legacyKey, out var legacy) || legacy.Type == JTokenType.Null)
                    continue;

                if (legacy.Type == JTokenType.Integer)
                    form["labId"] = LabForm.GetOrCreateForBelong(legacy.Value<int>(), belong);
                else
                    form["labId"] = LabForm.GetOrCreate(legacy.Value<string>(), belong);
                break;
            }

        }
    }

    private static void MigrateHierarchicalLabIds(JArray forms, string belong, string lv1Key, string lv2Key)
    {
        foreach (JObject form in forms)
        {
            if (TryNormalizeCurrentLabId(form, belong))
                continue;

            var hasLv1 = form.TryGetValue(lv1Key, out var lv1Token) && lv1Token.Type != JTokenType.Null;
            var hasLv2 = form.TryGetValue(lv2Key, out var lv2Token) && lv2Token.Type != JTokenType.Null;
            if (!hasLv1 && !hasLv2)
            {
                form["labId"] = LabForm.NoneId;
                continue;
            }

            form["labId"] = LabForm.GetOrCreate(
                hasLv1 ? lv1Token.Value<string>() : string.Empty,
                hasLv2 ? lv2Token.Value<string>() : string.Empty,
                string.Empty,
                belong);
        }
    }

    private static void FlattenEventProgramLabs(JArray forms)
    {
        if (!LabForm.DatasByBelong.TryGetValue(nameof(EventProgramDataForm), out var eventLabs))
            return;

        var oldLabs = eventLabs
            .Where(lab => lab != null && (!string.IsNullOrEmpty(lab.lv2Lab) || !string.IsNullOrEmpty(lab.lv3Lab)))
            .OrderBy(lab => lab.id)
            .ToList();
        var remappedIds = new Dictionary<int, int>();
        foreach (var lab in oldLabs)
        {
            var category = !string.IsNullOrEmpty(lab.lv1Lab) ? lab.lv1Lab :
                !string.IsNullOrEmpty(lab.lv2Lab) ? lab.lv2Lab : lab.lv3Lab;
            remappedIds[lab.id] = LabForm.GetOrCreate(category, nameof(EventProgramDataForm));
        }

        if (forms != null)
        {
            foreach (JObject form in forms)
            {
                if (form.TryGetValue("labId", out var labIdToken) && labIdToken.Type == JTokenType.Integer &&
                    remappedIds.TryGetValue(labIdToken.Value<int>(), out var newLabId))
                    form["labId"] = newLabId;
            }
        }

        foreach (var lab in oldLabs)
            LabForm.RemoveData(lab.id);
    }

    private static bool TryNormalizeCurrentLabId(JObject form, string belong)
    {
        if (!form.TryGetValue("labId", out var current) || current.Type != JTokenType.Integer)
            return false;

        var labId = current.Value<int>();
        if (labId == LabForm.NoneId)
            return true;
        if (!LabForm.TryGetData(labId, out _))
            return false;

        form["labId"] = LabForm.GetOrCreateForBelong(labId, belong);
        return true;
    }

    public void LoadMaterial(string folder)
    {
        MapTextureForm.ClearAuto();
        MapMaskForm.ClearAuto();
        var pathForm = folder + mapTextureFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(MapTextureForm), "label");
            foreach (var form in MapTextureForm.GetDatasByJa(forms))
            {
                MapTextureForm.AddData(form);
            }
        }



        pathForm = folder + mapMaskFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(MapMaskForm), "label");
            foreach (var form in MapMaskForm.GetDatasByJa(forms))
            {
                MapMaskForm.AddData(form);
            }
        }

    }
    public void LoadObject(string folder)
    {
        MapObjectForm.Clear();
        legacyObjectForms = null;
        if (folder == null)
            return;
        var path = folder + mapObjectFormFileName;
        if (!SaveAndLoad.Exist(path))
            return;
        legacyObjectForms = JArray.Parse(SaveAndLoad.Load<string>(path));
        MigrateFlatLabIds(legacyObjectForms, nameof(ObjectProductForm), "label");
    }

    // Load last among Product-derived tables: old stories need newly allocated
    // Object Product IDs only after Character/Skill/Item IDs have been reserved.
    public void LoadObjectProducts(string folder)
    {
        ObjectProductForm.Clear();
        ObjectProductForm.ResetReferences();
        if (folder == null)
            return;
        var path = folder + objectProductFormFileName;
        var forms = SaveAndLoad.Exist(path) ? JArray.Parse(SaveAndLoad.Load<string>(path)) : null;
        ObjectProductForm.LoadProducts(forms, legacyObjectForms);
    }

    public void LoadCharacter(string folder)
    {
        var pathForm = folder + characterParamFormFileName;
        CharacterParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in CharacterParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                CharacterParamForm.AddData(form);
            }
        }

        pathForm = folder + characterProductFormFileName;
        CharacterProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(CharacterProductForm), "label");
            foreach (var form in CharacterProductForm.GetDatasByJa(forms))
            {
                CharacterProductForm.AddData(form);
            }
        }

        foreach (var data in CharacterProductForm.DataByUid.Values)
        {
            if (data.size < 1)
                data.size = 1;
            var icon = data.avatarTex;

            foreach (var anim in data.animDic.Values)
            {
                foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
                {
                    foreach (AnimDirecton dir in Enum.GetValues(typeof(AnimDirecton)))
                    {
                        if (!anim.animClip.ContainsKey(dir))
                        {
                            anim.animClip[dir] = new List<CharacterAnimClipForm.Data>();
                        }


                    }
                }
            }
        }
    }
    public void LoadSkill(string folder)
    {
        var pathForm = folder + skillParamFormFileName;
        SkillParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in SkillParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                SkillParamForm.AddData(form);
            }
        }

        pathForm = folder + skillProductFormFileName;
        SkillProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(SkillProductForm), "label");
            foreach (var form in SkillProductForm.GetDatasByJa(forms))
            {
                SkillProductForm.AddData(form);
            }
        }


    }
    public void LoadItem(string folder)
    {
        var pathForm = folder + itemParamFormFileName;
        ItemParamForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ItemParamForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ItemParamForm.AddData(form);
            }
        }

        pathForm = folder + itemProductFormFileName;
        ItemProductForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(ItemProductForm), "label");
            foreach (var form in ItemProductForm.GetDatasByJa(forms))
            {
                ItemProductForm.AddData(form);
            }
        }


    }

    public void LoadEffect(string folder)
    {
        var pathForm = folder + effectFormFileName;
        EffectForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(EffectForm), "label");
            foreach (var form in EffectForm.GetDatasByJa(forms))
            {
                EffectForm.AddData(form);
            }
        }
    }

    public void LoadMission(string folder)
    {
        MissionForm.ClearAuto();
        if (folder == null)
            return;

        var pathForm = folder + missionFormFileName;
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateFlatLabIds(forms, nameof(MissionForm), "label", "lab");
            foreach (var form in MissionForm.GetDatasByJa(forms))
            {
                MissionForm.AddData(form);
            }
        }
    }

    public void LoadAsset(string folder)
    {
        var path = "";
        path = folder + imageAssetFormFileName;
        StoryTexAssetForm.ClearAuto();
        StoryAudioAssetForm.ClearAuto();
        StoryVideoAssetForm.ClearAuto();
        if (SaveAndLoad.Exist(path))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(path));
            MigrateFlatLabIds(forms, nameof(StoryTexAssetForm), "lab");
            foreach (var form in StoryTexAssetForm.GetDatasByJa(forms))
            {
                if (form != null)
                {
                    StoryTexAssetForm.AddData(form);
                    form.path = GetStoryAssetPath(folder, AssetManager.instance.texCtrl, form);
                }
            }
        }
        path = folder + audioAssetFormFileName;
        if (SaveAndLoad.Exist(path))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(path));
            MigrateFlatLabIds(forms, nameof(StoryAudioAssetForm), "lab");
            foreach (var form in StoryAudioAssetForm.GetDatasByJa(forms))
            {
                StoryAudioAssetForm.AddData(form);
                form.path = GetStoryAssetPath(folder, AssetManager.instance.audioCtrl, form);
            }
        }
        path = folder + videoAssetFormFileName;
        if (SaveAndLoad.Exist(path))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(path));
            MigrateFlatLabIds(forms, nameof(StoryVideoAssetForm), "lab");
            foreach (var form in StoryVideoAssetForm.GetDatasByJa(forms))
            {
                StoryVideoAssetForm.AddData(form);
                form.path = GetStoryAssetPath(folder, AssetManager.instance.videoCtrl, form);
            }
        }

    }

    public void LoadEvent(string folder)
    {
        var pathForm = folder + eventFormFileName;

        EventProgramDataForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateHierarchicalLabIds(forms, nameof(EventProgramDataForm), "category", "type");
            FlattenEventProgramLabs(forms);
            MigrateObjectReferences(forms);
            var programs = EventProgramDataForm.GetDatasByJa(forms);
            foreach (var form in programs)
            {
                EventProgramDataForm.AddData(form);
            }
            // Load every program first, so cross-program calls can be resolved while recompiling.
            foreach (var form in programs)
            {
                var migratedCode = MigrateLegacyEventParameters(form.code);
                if (migratedCode == form.code && form.zCode != null && form.zCode.Count > 0)
                    continue;
                if (!form.TryApplyCode(migratedCode, out _, out var errors))
                {
                    Debug.LogWarning($"Event parameter migration failed for {form.name} ({form.uid}): {string.Join("; ", errors)}");
                }
            }
        }
        else if (folder != null)
            FlattenEventProgramLabs(null);


    }
    public MapInfo LoadSceneMap(string scenePath)
    {
        var mapData = new GameMapData();
        var json = SaveAndLoad.Load<string>(scenePath);
        // Keep migration in memory when Mod reads authored Core directly.
        mapData.Init(ObjectProductForm.MigrateSceneJson(json, false));
        return mapData;
    }
    public void LoadProgress(string folder)
    {
        var pathForm = folder + progressFormFileName;

        if (SaveAndLoad.Exist(pathForm))
        {
            var forms = JArray.Parse(SaveAndLoad.Load<string>(pathForm));
            MigrateObjectReferences(forms);
            foreach (var form in ProgressForm.GetDatasByJa(forms))
            {
                ProgressForm.AddData(form);
            }
            DynamicGlobalSettings.cameraMode = GameManager.instance.curProgress.cameraMode;
            DynamicGlobalSettings.antiOcclusion = GameManager.instance.curProgress.antiOcclusion;
            DynamicGlobalSettings.allowDrop = GameManager.instance.curProgress.allowDrop;
            DynamicGlobalSettings.edgeTransition = GameManager.instance.curProgress.edgeTransition;
            GameManager.ApplyProgressLightIntensity();
        }
    }

    private bool RepairSavedCurrentCharacter()
    {
        var progress = GameManager.instance.curProgress;
        if (progress == null || CharacterProductForm.DataByUid.ContainsKey(progress.characterUid))
            return false;
        // Undeploying the final member deliberately leaves a reserve-only team.
        // A missing list or an invalid nonzero current UID still uses legacy repair.
        if (progress.characterUid == 0 && progress.teamActive?.Count == 0)
            return false;

        var replacementUid = 0;
        if (progress.teamActive != null)
        {
            foreach (var uid in progress.teamActive)
            {
                if (CharacterProductForm.DataByUid.ContainsKey(uid))
                {
                    replacementUid = uid;
                    break;
                }
            }
        }
        if (replacementUid == 0 && progress.team != null)
        {
            foreach (var uid in progress.team)
            {
                if (CharacterProductForm.DataByUid.ContainsKey(uid))
                {
                    replacementUid = uid;
                    break;
                }
            }
        }
        if (replacementUid == 0)
            return false;

        progress.characterUid = replacementUid;
        if (progress.team == null)
            progress.team = new List<int>();
        if (!progress.team.Contains(replacementUid))
            progress.team.Add(replacementUid);
        if (progress.teamActive == null)
            progress.teamActive = new List<int>();
        if (!progress.teamActive.Contains(replacementUid))
            progress.teamActive.Add(replacementUid);
        Debug.LogWarning($"Repaired saved current character to UID {replacementUid}.");
        return true;
    }


    public void LoadUiItem(string folder)
    {
        var pathForm = folder + imageUiItemFormFileName;
        ImageUiItemForm.Clear();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in ImageUiItemForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                ImageUiItemForm.AddData(form);
            }
        }
    }

    public void LoadScene(string folder)
    {
        var pathForm = folder + sceneFormFileName;
        SceneForm.ClearAuto();
        if (SaveAndLoad.Exist(pathForm))
        {
            foreach (var form in SceneForm.GetDatasByJa(JArray.Parse(SaveAndLoad.Load<string>(pathForm))))
            {
                SceneForm.AddData(form);
            }
        }
    }

    public void LoadLocalModStory()
    {

    }
    #region util

    public void RepairPassTypeReferences()
    {
        foreach (var texture in MapTextureForm.DataById.Values)
        {
            if (texture.passType != 0 && !PassTypeForm.DataById.ContainsKey(texture.passType))
                texture.passType = 0;
        }

        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            if (character.passType == null)
                character.passType = new List<int>();
            character.passType.RemoveAll(id => id == 0 || !PassTypeForm.DataById.ContainsKey(id));
            character.passType = character.passType.Distinct().ToList();
        }

        foreach (var tile in TileUnitForm.DataByUid.Values)
            GameMapData.ApplyTilePassTypes(tile);
        foreach (var character in CharacterUnitForm.DataByUid.Values)
            GameMapData.ApplyCharacterProductPassTypes(character);
    }

    private static string MigrateLegacyEventParameters(string code)
    {
        if (string.IsNullOrEmpty(code))
            return code;
        // Preserve quoted text; only standalone identifiers are event parameters.
        return Regex.Replace(code, "\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*'|(?<![A-Za-z0-9_.])(?:self|target|delta)(?![A-Za-z0-9_])", match =>
        {
            switch (match.Value)
            {
                case "self": return "param1";
                case "target": return "param2";
                case "delta": return "param3";
                default: return match.Value;
            }
        });
    }

    // Upgrade both source code and saved interpreter/string values. Replacing
    // complete marker pairs leaves surrounding quotes and unrelated text intact.
    private static void MigrateObjectReferences(JToken token)
    {
        var tokens = token is JContainer container ? container.DescendantsAndSelf() : new[] { token };
        foreach (var value in tokens.OfType<JValue>())
        {
            if (value.Type != JTokenType.String)
                continue;
            value.Value = Regex.Replace((string)value, @"\$so\$t:([0-9]+)\$so\$|\$sop\$([0-9]+)\$sop\$", match =>
            {
                string digits = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                if (!int.TryParse(digits, out int mapId))
                    return match.Value;
                var product = ObjectProductForm.ResolveLegacyTemplate(mapId);
                return product == null ? match.Value : "$so$p:" + product.uid + "$so$";
            });
        }
    }


    public void RepairMissingCharacterTextureReferences()
    {
        const int missingPartTextureId = int.MinValue;
        var missingReferences = new List<(Dictionary<BodyPartType, int> partTex, BodyPartType part, int missingId)>();

        foreach (var character in CharacterProductForm.DataByUid.Values)
        {
            if (character.animDic == null)
                continue;

            foreach (var anim in character.animDic.Values)
            {
                if (anim?.animClip == null)
                    continue;

                foreach (var clips in anim.animClip.Values)
                {
                    if (clips == null)
                        continue;

                    foreach (var clip in clips)
                    {
                        if (clip == null)
                            continue;
                        if (clip.partTex == null)
                            clip.partTex = new Dictionary<BodyPartType, int>();

                        foreach (BodyPartType part in Enum.GetValues(typeof(BodyPartType)))
                        {
                            var hasTextureId = clip.partTex.TryGetValue(part, out var textureId);
                            if (!hasTextureId || !TexAssetForm.DataById.ContainsKey(textureId))
                            {
                                missingReferences.Add((
                                    clip.partTex,
                                    part,
                                    hasTextureId ? textureId : missingPartTextureId));
                            }
                        }
                    }
                }
            }
        }

        if (missingReferences.Count == 0)
            return;

        var replacements = new Dictionary<int, int>();
        var emptyTextureBytes = TextureHelper.GetTextureByte(TextureHelper.transparentTexture);
        foreach (var missingReference in missingReferences)
        {
            if (!replacements.TryGetValue(missingReference.missingId, out var replacementId))
            {
                var missingName = missingReference.missingId == missingPartTextureId
                    ? "missing_texture"
                    : $"missing_texture_{missingReference.missingId}";
                var rawData = AssetManager.instance.texCtrl.CreateDataByBytes(emptyTextureBytes, missingName);
                rawData.labId = LabForm.NoneId;
                var replacement = AddStoryTexInternal(rawData);
                replacementId = replacement.id;
                replacements[missingReference.missingId] = replacementId;
            }

            missingReference.partTex[missingReference.part] = replacementId;
        }
    }

    private StoryTexAssetForm.Data AddStoryTexInternal(TexAssetForm.Data rawData)
    {
        var data = new StoryTexAssetForm.Data(rawData);
        if (StoryTexAssetForm.DataById.ContainsKey(data.id))
        {
            var oldData = StoryTexAssetForm.DataById[data.id];
            StoryTexAssetForm.RemoveData(oldData.id);
        }
        data.labId = LabForm.GetOrCreateForBelong(data.labId, nameof(StoryTexAssetForm));
        StoryTexAssetForm.AddData(data);
        return data;
    }

    public void AddStoryTex(ref TexAssetForm.Data rawData)
    {
        var data = AddStoryTexInternal(rawData);
        rawData = data;

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });

    }
    public void AddGameTex(ref TexAssetForm.Data rawData)
    {
        GameTexAssetForm.Data data = new GameTexAssetForm.Data(rawData);
        if (GameTexAssetForm.DataById.ContainsKey(data.id))
        {
            var oldData = GameTexAssetForm.DataById[data.id];
            GameTexAssetForm.RemoveData(oldData.id);
        }
        data.labId = LabForm.GetOrCreateForBelong(data.labId, nameof(GameTexAssetForm));
        GameTexAssetForm.AddData(data);

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });
        rawData = data; 
    }

    public void AddStoryAudio(AudioAssetForm.Data rawData)
    {
        StoryAudioAssetForm.Data data = new StoryAudioAssetForm.Data(rawData);
        if (StoryAudioAssetForm.DataById.ContainsKey(data.id))
        {
            var oldData = StoryAudioAssetForm.DataById[data.id];
            StoryAudioAssetForm.RemoveData(oldData.id);
        }
        data.labId = LabForm.GetOrCreateForBelong(data.labId, nameof(StoryAudioAssetForm));
        StoryAudioAssetForm.AddData(data);

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });
    }

    public void AddStoryVideo(VideoAssetForm.Data rawData)
    {
        StoryVideoAssetForm.Data data = new StoryVideoAssetForm.Data(rawData);
        if (StoryVideoAssetForm.DataById.ContainsKey(data.id))
        {
            var oldData = StoryVideoAssetForm.DataById[data.id];
            StoryVideoAssetForm.RemoveData(oldData.id);
        }
        data.labId = LabForm.GetOrCreateForBelong(data.labId, nameof(StoryVideoAssetForm));
        StoryVideoAssetForm.AddData(data);

        Z_EventHelper.Invoke(new AssetEvent()
        {
            importAssetName = data.name
        });
    }
    #endregion
    #endregion

    #region copy
    public void CopyMapScene(string from, string to)
    {
        foreach (var data in SceneForm.DataByUid.Values)
        {
            SaveAndLoad.Copy(from + Main2StoryManager.GetSceneFileNameById(data.uid), to + Main2StoryManager.GetSceneFileNameById(data.uid));
        }
    }

    #endregion

    #region check
    public bool LackMapScene(string folder)
    {
        foreach (var data in SceneForm.DataByUid.Values)
        {
            if (!SaveAndLoad.Exist(folder + Main2StoryManager.GetSceneFileNameById(data.uid)))
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region del
    public void DeleteCache(int id)
    {
        SaveAndLoad.Delete(Main2StoryManager.GetStoryCacheFolder(id));
    }
    public void DeleteSceneMap(string storyCoreFolder, int id)
    {

        SaveAndLoad.Delete(storyCoreFolder + "/" + id);
    }

    #endregion

    public void ResetPrefabPool()
    {
        InstancePoolManager.instance.Clear();
        foreach (Transform child in InstancePoolManager.instance.defaultRoot)
        {
            GameObject.Destroy(child.gameObject); // 销毁子物体
        }
        foreach (var form in GameObjectAssetForm.DataById.Values)
        {
            if (form.name.StartsWith(GlobalDefaultHelper.GetInternalPrefabName("")))
            {
                InstancePoolManager.instance.AddPool(form.GetGo());
            }
        }
        MapObjectForm.Clear();
        if (ObjectProductForm.DatasByProtouid.TryGetValue(0, out var prototypes))
            foreach (var product in prototypes.OrderBy(product => product.uid))
                MapObjectForm.AddData(new MapObjectForm.Data(-1, product.name, product.icon,
                    LabForm.GetOrCreateForBelong(product.labId, nameof(MapObjectForm)), product.uid));
        foreach (var form in ObjectProductForm.DataByUid.Values.Where(product => product.protoUid == 0))
        {
            var obj = _super.utilCtrl.CombineNewObjectByPrefabs(GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(form.uid), form, true);
            obj.transform.parent = InstancePoolManager.instance.defaultRoot;
            InstancePoolManager.instance.AddPool(obj);
        }

        //item生成mapItem
        MapItemForm.Clear();
        if (ItemProductForm.DatasByProtouid.ContainsKey(0))
        {
            foreach (var itemData in ItemProductForm.DatasByProtouid[0])
            {
                var labId = LabForm.GetOrCreateForBelong(itemData.labId, nameof(MapItemForm));
                MapItemForm.AddData(new MapItemForm.Data(-1, itemData.name, itemData.iconTexName, itemData.model, labId, itemData.uid));
            }
        }

        //character生成mapCharacter
        MapCharacterForm.Clear();
        if (CharacterProductForm.DatasByProtouid.ContainsKey(0))
        {
            foreach (var characterData in CharacterProductForm.DatasByProtouid[0])
            {
                var labId = LabForm.GetOrCreateForBelong(characterData.labId, nameof(MapCharacterForm));
                MapCharacterForm.AddData(new MapCharacterForm.Data(-1, characterData.name, characterData.avatarTex, labId, characterData.uid));
            }
        }

        foreach (var form in ItemProductForm.DataByUid.Values)
        {
            var obj = _super.utilCtrl.CombineNewObjectByPrefabs(GlobalDefaultHelper.GetRuntimeMapItemPrefabName(form.uid), form.model, true, true);
            obj.transform.parent = InstancePoolManager.instance.defaultRoot;
            InstancePoolManager.instance.AddPool(obj);
        }

        var character = _super.utilCtrl.CombineNewCharacterByPrefabs(GlobalDefaultHelper.GetRuntimePrefabName("character"), new List<int>() { GlobalDefaultHelper.DefaultTexId, GlobalDefaultHelper.DefaultTexId, -1 }, true);
        character.transform.parent = InstancePoolManager.instance.defaultRoot;
        InstancePoolManager.instance.AddPool(character);

    }


}
