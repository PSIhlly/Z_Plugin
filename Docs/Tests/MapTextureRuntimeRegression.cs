using System;
using System.Collections;
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

// Isolated Editor fixture; never load the real game, story, or saves.
public static class MapTextureRuntimeRegression
{
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private static int checks;
    private static readonly MaterialPropertyBlock readBlock = new MaterialPropertyBlock();

    public static void Run()
    {
        try
        {
            PerspectiveImageScaleAfterRotation();
            // RuntimeInitializeOnLoadMethod registration does not run in EditMode.
            TexAssetForm.InitInternal();
            MapTextureForm.InitInternal();
            MapMaskForm.InitInternal();
            var map = CreateManager<MapManager>();
            map.data = new MapInfo { maps = new Dictionary<(int, int, int), TileUnitForm.Data>() };
            map.utilCtrl = new MapUtilController(map);
            var time = CreateManager<TimeManager>();
            time.gameObject.SetActive(true);
            var ctrl = new GameMapController(null);
            ctrl.Reset();
            var a = AddTexture(81001);
            var b = AddTexture(81002);
            var mask = AddTexture(81003);
            var replacement = new Texture2D(2, 2);
            var tile = NewTile(82001);
            var root = new GameObject("texture-regression");
            for (int i = 0; i < 6; i++)
            {
                var child = new GameObject("renderer-" + i);
                child.transform.SetParent(root.transform, false);
                child.AddComponent<MeshRenderer>();
            }
            var ins = root.AddComponent<TileInstance>();
            tile.unit.ins = ins;
            ins.unit = tile.unit;
            var renderer = ins.renderers[0];
            readBlock.SetFloat("_Show", 1f);
            readBlock.SetFloat("_FadeCenter", 1f);
            readBlock.SetFloat("_LightSensitivity", .3f);
            renderer.SetPropertyBlock(readBlock);

            var baseFrames = new List<int> { 81001 };
            var frontFrames = new List<int> { 81002 };
            var product = new MapTextureForm.Data(83001, "test", 81001, 0, baseFrames, 0,
                new Dictionary<string, EventTriggerForm.Data>(), false, new Dictionary<int, int>(), 0,
                true, frontFrames, false, new Dictionary<int, int>());
            MapTextureForm.DataById[product.id] = product;
            var maskProduct = new MapMaskForm.Data(83002, "mask", 81003, new List<int> { 81003 }, 0);
            MapMaskForm.DataById[maskProduct.id] = maskProduct;
            tile.texDic[0] = product.id;
            tile.texDic[3] = maskProduct.id;
            ctrl.alphaTextureDic[(81003, 0)] = mask;
            var evt = new TileEvent { type = MapEventType.Show, unit = tile.unit };
            ctrl.OnEvent(evt);
            Check(Texture(renderer, "_Tex") == a && Texture(renderer, "_AlphaTex") == mask,
                "base and mask compose without replacing base texture");
            Check(Texture(ins.renderers[3], "_Tex") == b, "front has its independent texture");
            Check(!ins.renderers[1].enabled && !ins.renderers[4].enabled, "empty base/front disabled");
            renderer.GetPropertyBlock(readBlock);
            Check(readBlock.GetFloat("_Show") == 1f && readBlock.GetFloat("_FadeCenter") == 1f
                && readBlock.GetFloat("_LightSensitivity") == .3f,
                "texture writes preserve camera-center fade, visibility and lighting");
            var blockField = typeof(GameMapController).GetField("textureBlock", PrivateInstance);
            var scratch = blockField.GetValue(ctrl);
            ctrl.OnEvent(evt);
            Check(ReferenceEquals(scratch, blockField.GetValue(ctrl)), "scratch block reused across all layers");

            tile.texDic.Remove(0);
            ctrl.OnEvent(evt);
            Check(!renderer.enabled && !ins.renderers[3].enabled, "retained mask cannot enable removed layer");
            tile.texDic[0] = product.id;
            tile.texDic.Remove(3);
            ctrl.OnEvent(evt);
            Check(renderer.enabled && Texture(renderer, "_AlphaTex") == Texture2D.whiteTexture,
                "removing mask resets white and layer re-enables");
            product.enableFrontPart = false;
            ctrl.OnEvent(evt);
            Check(!ins.renderers[3].enabled, "disabled front clears previous active state");
            product.enableFrontPart = true;
            ins.renderers[3].gameObject.SetActive(false);
            ctrl.OnEvent(evt);
            Check(ins.renderers[3].gameObject.activeSelf && ins.renderers[3].enabled, "inactive authored front activates");

            var mixed = new List<int> { 89999, 81001, 81002 };
            ctrl.ShowFinalMat(ins, 0, mixed, 0);
            Check(Texture(renderer, "_Tex") == a, "missing frames skipped in source order");
            mixed[1] = 81002;
            ctrl.ShowFinalMat(ins, 0, mixed, 0);
            Check(Texture(renderer, "_Tex") == b, "same-length in-place edits invalidate cached frames");
            var removed = TexAssetForm.DataById[81002];
            TexAssetForm.DataById.Remove(81002);
            ctrl.ShowFinalMat(ins, 0, mixed, 0);
            Check(renderer.enabled && Texture(renderer, "_Tex") != b, "all missing frames use default, not stale texture");
            TexAssetForm.DataById[81002] = removed;
            ctrl.ShowFinalMat(ins, 0, mixed, 0);
            Check(Texture(renderer, "_Tex") == b, "restored IDs invalidate filtered frame cache");
            removed.asset = replacement;
            ctrl.ShowFinalMat(ins, 0, mixed, 0);
            Check(Texture(renderer, "_Tex") == replacement, "same-ID asset replacement is not texture-cached");
            removed.asset = b;

            product.isWangTile = true;
            product.frontIsWangTile = true;
            product.WangTileDic[0] = 81002;
            product.FrontWangTileDic[0] = 81001;
            ctrl.OnEvent(evt);
            Check(Texture(renderer, "_Tex") == b && Texture(ins.renderers[3], "_Tex") == a,
                "single Wang variants use independent inline IDs without lists");
            var register = typeof(GameMapController).GetMethod("RegisterWangTileAnimationTexture", PrivateStatic);
            register.Invoke(null, new object[] { product.id, false, 0, 81001 });
            register.Invoke(null, new object[] { product.id, false, 0, 81002 });
            ctrl.OnEvent(evt);
            Check(Texture(renderer, "_Tex") == a, "registered Wang sequence selected before single variant");

            // Warm the complete static Tile Show route and filtered sequence route.
            for (int i = 0; i < 20; i++) { ctrl.OnEvent(evt); ctrl.ShowFinalMat(ins, 0, mixed, 0); }
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 100; i++) { ctrl.OnEvent(evt); ctrl.ShowFinalMat(ins, 0, mixed, 0); }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Check(allocated == 0, "warmed static/Wang/missing-frame display paths allocate zero managed bytes: " + allocated);

            // Drive the exact callback helper directly in EditMode; real scheduling
            // still requires Play-mode smoke testing.
            var framesType = typeof(GameMapController).GetNestedType("TextureFrames", BindingFlags.NonPublic);
            var animation = new List<int> { 81001, 81002 };
            var frames = Activator.CreateInstance(framesType, new object[] { animation });
            var tick = typeof(GameMapController).GetMethod("UpdateTextureAnimation", PrivateInstance);
            var frameIndices = new Dictionary<int, int>();
            TimeManager.animationTimeGetter = () => 1.25f;
            tile.texDic[3] = maskProduct.id;
            product.animTimeInterval = .5f;
            ctrl.OnEvent(evt);
            Check(ins.animTimer[0] != null && !ins.animTimer[0].cancel
                && Texture(renderer, "_Tex") == a && Texture(renderer, "_AlphaTex") == mask,
                "merged Tile Show retains both shared-phase animation and final mask");
            ctrl.ShowFinalMat(ins, 0, animation, .5f);
            var timer = ins.animTimer[0];
            ctrl.ShowFinalMat(ins, 0, maskProduct.texsName, 0, true);
            Check(timer != null && !timer.cancel && ReferenceEquals(timer, ins.animTimer[0]),
                "mask-only update keeps the running base animation");
            TimeManager.animationTimeGetter = () => 1.75f;
            tick.Invoke(ctrl, new object[] { ins, tile.unit, renderer, 0, frames, .5f, frameIndices });
            Check(Texture(renderer, "_Tex") == b && Texture(renderer, "_AlphaTex") == mask,
                "animation selects shared-clock frame and preserves mask");
            renderer.GetPropertyBlock(readBlock);
            Check(readBlock.GetFloat("_Show") == 1f && readBlock.GetFloat("_FadeCenter") == 1f
                && ReferenceEquals(scratch, blockField.GetValue(ctrl)),
                "animation reuses block and preserves camera-center fade");
            animation[1] = 81001;
            tick.Invoke(ctrl, new object[] { ins, tile.unit, renderer, 0, frames, .5f, frameIndices });
            Check(Texture(renderer, "_Tex") == a, "animation observes edits even at the same clock frame");

            var newTile = NewTile(82002);
            ins.unit = newTile.unit;
            renderer.SetPropertyBlock(null);
            Check((bool)tick.Invoke(ctrl, new object[] { ins, tile.unit, renderer, 0, frames, .5f, frameIndices }),
                "old-owner animation terminates after pool rebind");
            ctrl.ShowFinalMat(ins, 0, frontFrames, 0);
            Check(timer.cancel && ins.animTimer[0] == null && Texture(renderer, "_Tex") == b,
                "pool rebind restores texture and cancels old animation");
            ctrl.ShowFinalMat(ins, 0, null, 0);
            ctrl.ShowFinalMat(ins, 0, maskProduct.texsName, 0, true);
            Check(!renderer.enabled && Texture(renderer, "_AlphaTex") == Texture2D.blackTexture,
                "mask-only path keeps empty pooled layer disabled");
            var cache = (IDictionary)typeof(GameMapController).GetField("validatedTextureFrames", PrivateStatic).GetValue(null);
            Check(cache.Count > 0, "multi-frame validation cache populated");
            ctrl.Reset();
            Check(cache.Count == 0 && ctrl.animCurCache.Count == 0, "reset releases frame caches between stories");
            TimeManager.animationTimeGetter = null;
            Debug.Log("MAP_TEXTURE_RUNTIME_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static Texture2D AddTexture(int id)
    {
        var texture = new Texture2D(2, 2);
        TexAssetForm.DataById[id] = new TexAssetForm.Data(id, "test", "", null, "", texture, 0);
        return texture;
    }

    private static TileUnitForm.Data NewTile(int uid)
    {
        return new TileUnitForm.Data(uid, "", new Dictionary<int, int>(), Vector3Int.zero,
            "", Vector3.zero, Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), "", false, false, new List<int>());
    }

    private static void PerspectiveImageScaleAfterRotation()
    {
        var previousMode = DynamicGlobalSettings.cameraMode;
        var root = new GameObject("perspective-object-instance");
        try
        {
            var primitive = new GameObject("MapPrefab$cube");
            primitive.transform.SetParent(root.transform, false);
            var body = new GameObject("Collider");
            body.transform.SetParent(primitive.transform, false);
            var collider = body.AddComponent<BoxCollider>();
            var holder = new GameObject("rotateHolder");
            holder.transform.SetParent(primitive.transform, false);
            var image = new GameObject("img");
            image.transform.SetParent(holder.transform, false);
            var authoredScale = new Vector3(.8f, 1.2f, 1f);
            image.transform.localScale = authoredScale;
            var keeper = image.AddComponent<PerspectiveKeeper>();
            keeper.rotateHolder = holder.transform;
            keeper.enableFixedXRotationDefault = true;
            keeper.deepth = .07f;

            foreach (var rootScale in new[] { Vector3.one, new Vector3(2, 3, .5f) })
            foreach (var modelScale in new[] { Vector3.one, new Vector3(1, 10, 1), new Vector3(2, 1, 8), new Vector3(3, .25f, .1f) })
            foreach (float yaw in new[] { 0f, 37f, 90f, 180f, 270f })
            foreach (var mode in new[] { CameraMode.Isometric, CameraMode.Overhead, CameraMode.Isometric })
            {
                root.transform.localScale = rootScale;
                root.transform.rotation = Quaternion.Euler(8, yaw, 7);
                primitive.transform.localScale = modelScale;
                DynamicGlobalSettings.cameraMode = mode;
                Vector3 size = primitive.transform.lossyScale;
                var rotation = Quaternion.Euler(mode == CameraMode.Isometric ? 45f : 90f, 0, 0)
                    * Quaternion.AngleAxis(-primitive.transform.eulerAngles.y, Vector3.forward);
                float imageHeight = mode == CameraMode.Isometric
                    ? Mathf.Sqrt(size.y * size.y + size.z * size.z) : Mathf.Abs(size.z);
                for (int repeat = 0; repeat < 2; repeat++)
                {
                    keeper.RefreshNow();
                    Check(image.transform.localRotation == Quaternion.identity && holder.transform.localScale == Vector3.one,
                        "outer rotateHolder owns rotation, inner img owns scale");
                    Check(holder.transform.parent.name == "imageScaleCompensator" && primitive.transform.childCount == 2,
                        "one owned scale compensator is reused without changing model/Collider hierarchy");
                    var expectedPosition = mode == CameraMode.Isometric
                        ? primitive.transform.position + new Vector3(0, 1, -1) * keeper.deepth
                        : primitive.transform.position + Vector3.up * primitive.transform.localScale.y / 2 + Vector3.down * keeper.deepth;
                    Check((holder.transform.position - expectedPosition).sqrMagnitude < .000001f,
                        "scale compensation never rescales the authored world depth/height offset");
                    Vector3 actualRight = image.transform.TransformVector(Vector3.right);
                    Vector3 actualUp = image.transform.TransformVector(Vector3.up);
                    Check(Vector3.Angle(actualRight, rotation * Vector3.right) < .05f,
                        "model/holder scale must not change the actual image right direction");
                    Check(Vector3.Angle(actualUp, rotation * Vector3.up) < .05f,
                        "model/holder scale must not change the actual image up direction or pitch");
                    Check(Mathf.Abs(Vector3.Dot(actualRight.normalized, actualUp.normalized)) < .0001f,
                        "the rendered image plane remains rectangular rather than sheared");
                    Check(Mathf.Abs(actualRight.magnitude - Mathf.Abs(size.x) * authoredScale.x) < .0001f
                        && Mathf.Abs(actualUp.magnitude - imageHeight * authoredScale.y) < .0001f,
                        "scale applies along rotated image axes and preserves width/diagonal height without accumulation");
                    Check(body.transform.localPosition == Vector3.zero && body.transform.localScale == Vector3.one
                        && collider.center == Vector3.zero && collider.size == Vector3.one,
                        "image transform correction never modifies the physical Collider");
                }
            }
            var compensator = holder.transform.parent;
            root.SetActive(false);
            root.SetActive(true);
            keeper.LateUpdate();
            Check(holder.transform.parent == compensator && primitive.transform.childCount == 2,
                "pool re-enable reuses the same neutral carrier");
            var refreshedScale = image.transform.localScale;
            var clone = UnityEngine.Object.Instantiate(root);
            try
            {
                var clonedKeeper = clone.GetComponentInChildren<PerspectiveKeeper>();
                clonedKeeper.RefreshNow();
                Check((clonedKeeper.transform.localScale - refreshedScale).sqrMagnitude < .000001f
                    && clonedKeeper.rotateHolder.parent.parent.childCount == 2,
                    "cloning a refreshed hierarchy retains the authored image scale and existing carrier");
            }
            finally { UnityEngine.Object.DestroyImmediate(clone); }

            // Characters share this helper through the sphere prefab. Their
            // diameter-sized square and direction must remain unchanged.
            var sphereRoot = new GameObject("MapPrefab$sphere");
            sphereRoot.transform.SetParent(root.transform, false);
            root.transform.localScale = Vector3.one * 2;
            root.transform.rotation = Quaternion.Euler(0, 37, 0);
            sphereRoot.transform.localScale = new Vector3(1, 2, 1);
            var sphereCollider = sphereRoot.AddComponent<SphereCollider>();
            sphereCollider.radius = .4f;
            var sphereHolder = new GameObject("rotateHolder");
            sphereHolder.transform.SetParent(sphereRoot.transform, false);
            var sphereImage = new GameObject("img");
            sphereImage.transform.SetParent(sphereHolder.transform, false);
            var sphereKeeper = sphereImage.AddComponent<PerspectiveKeeper>();
            sphereKeeper.rotateHolder = sphereHolder.transform;
            sphereKeeper.RefreshNow();
            Vector3 sphereRight = sphereImage.transform.TransformVector(Vector3.right);
            Vector3 sphereUp = sphereImage.transform.TransformVector(Vector3.up);
            var sphereRotation = Quaternion.Euler(45, 0, 0) * Quaternion.AngleAxis(-37, Vector3.forward);
            Check(Vector3.Angle(sphereRight, sphereRotation * Vector3.right) < .05f
                && Vector3.Angle(sphereUp, sphereRotation * Vector3.up) < .05f
                && Mathf.Abs(sphereRight.magnitude - 4) < .0001f && Mathf.Abs(sphereUp.magnitude - 4) < .0001f,
                "sphere/character keeps its original diameter square and image-plane direction");
            Check(sphereCollider.radius == .4f && sphereRoot.transform.localScale == new Vector3(1, 2, 1),
                "sphere visual correction never resizes the gameplay body or authored model");
        }
        finally
        {
            DynamicGlobalSettings.cameraMode = previousMode;
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static Texture Texture(Renderer renderer, string property)
    {
        renderer.GetPropertyBlock(readBlock);
        return readBlock.GetTexture(property);
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
