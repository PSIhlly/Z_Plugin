using Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Math;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;

public class GameUtilController : Z_Controller<GameManager>
{
    private const float CollisionBoundsAlphaThreshold = 0.3f;

    public int emptyTexId => GlobalDefaultHelper.DefaultTexId;
    public GameUtilController(GameManager super) : base(super)
    {
    }


    public GameObject CombineNewCharacterByPrefabs(string name, List<int> texRealId, bool forGame)
    {

        List<bool> showShaddowLst = new List<bool>()
                {
                    false,false
                };

        var res = CombineNewGoByPrefabs(name, new List<int>() { _super.innerAssetDic["sphere"].id, _super.innerAssetDic["sphere"].id }, texRealId, new List<Vector3>() { Vector3.zero, Vector3.zero }, new List<Vector3>() { Vector3.one, Vector3.one }, showShaddowLst);
        var renders = res.GetComponentsInChildren<Renderer>();
        renders[0].transform.GetComponent<PerspectiveKeeper>().deepth = 0.01f;
        renders[1].transform.GetComponent<PerspectiveKeeper>().deepth = 0.05f;

        res.GetComponentsInChildren<SphereCollider>()[0].radius = 0.4f;
        GameObject.Destroy(res.GetComponentsInChildren<SphereCollider>()[2].gameObject);
        if (forGame)
        {
            res.AddComponent<CharacterInstance>();
            //default disable
            foreach (var r in renders)
            {
                r.enabled = false;
            }

        }
        return res;
    }
    public GameObject CombineNewObjectByPrefabs(string name, MapObjectForm.Data product, bool forGame,
        AnimDirecton? previewDirection = null)
    {
        var boundsTextures = product.boundsCollision && !forGame
            ? product.GetAnimClip(previewDirection ?? product.GetDefaultAnimDirection()) : null;
        // Pool first parts retain a full baseline. Each unit fits only its active
        // facing (and assembled WangTile mask), never the union of other facings.
        Rect? firstPartBounds = product.boundsCollision
            ? forGame ? new Rect(0, 0, 1, 1)
                : product.isWangTile ? GetOpaqueWangTileBounds(boundsTextures, 0)
                    : GetOpaqueTextureBounds(boundsTextures, new Dictionary<Texture2D, Rect>())
            : (Rect?)null;
        return CombineNewObjectByPrefabs(name, product.model, forGame,
            boundsCollision: product.boundsCollision, firstPartBoundsTextures: boundsTextures,
            firstPartBounds: firstPartBounds, centerCollider: product.centerCollider);
    }

    public GameObject CombineNewObjectByPrefabs(string name, MapModelForm.Data model, bool forGame, bool isItem = false,
        bool boundsCollision = false, List<int> firstPartBoundsTextures = null, Rect? firstPartBounds = null,
        bool centerCollider = true)
    {
        if (model == null)
            return new GameObject(name);

        var prefabKeys = model.subPrefabUnitName ?? new List<int>();
        var showShadow = new List<bool>();
        var texRealId = new List<int>();
        var poss = new List<Vector3>();
        var scales = new List<Vector3>();
        for (int i = 0; i < prefabKeys.Count; i++)
        {
            showShadow.Add(true);
            texRealId.Add(model.subUnitTexsName != null && i < model.subUnitTexsName.Count &&
                          model.subUnitTexsName[i] != null && model.subUnitTexsName[i].Count > 0
                ? model.subUnitTexsName[i][0]
                : GlobalDefaultHelper.DefaultTexId);
            poss.Add(model.subPrefabUnitPos != null && i < model.subPrefabUnitPos.Count
                ? model.subPrefabUnitPos[i]
                : Vector3.zero);
            scales.Add(model.subPrefabUnitScale != null && i < model.subPrefabUnitScale.Count
                ? model.subPrefabUnitScale[i]
                : Vector3.one);
        }
        var res = CombineNewGoByPrefabs(name, prefabKeys, texRealId, poss, scales, showShadow);
        foreach (var col in res.GetComponentsInChildren<BoxCollider>())
        {
            col.transform.localScale = new Vector3(model.colliderScale, 1, model.colliderScale);
        }
        foreach (var col in res.GetComponentsInChildren<SphereCollider>())
        {
            col.transform.localScale = new Vector3(model.colliderScale, model.colliderScale, model.colliderScale);
        }
        if (boundsCollision)
        {
            // Only scan during prefab creation, never on animation ticks or movement.
            var textureBounds = new Dictionary<Texture2D, Rect>();
            for (int i = 0; i < prefabKeys.Count; i++)
            {
                var frames = i == 0 && firstPartBoundsTextures != null && firstPartBoundsTextures.Count > 0
                    ? firstPartBoundsTextures
                    : model.subUnitTexsName != null && i < model.subUnitTexsName.Count
                        ? model.subUnitTexsName[i] : null;
                var bounds = i == 0 && firstPartBounds.HasValue ? firstPartBounds.Value
                    : GetOpaqueTextureBounds(frames, textureBounds);
                FitHorizontalColliders(res.transform.GetChild(i).gameObject, bounds, model.colliderScale);
            }
        }
        if (forGame)
        {
            if (isItem)
                res.AddComponent<ItemInstance>();
            else
                res.AddComponent<ObjectInstance>();

        }

        // Runtime pools remain centered baselines: each unit may have its own
        // fitted facing/mask. Previews can apply the final rear anchor immediately.
        if (!forGame && !centerCollider)
            ApplyFirstPartCollisionBounds(res, res, new Rect(0, 0, 1, 1), model.colliderScale, false);

        return res;
    }

    internal static Rect GetOpaqueTextureBounds(List<int> frames, Dictionary<Texture2D, Rect> cache)
    {
        bool hasTexture = false;
        bool hasPixels = false;
        var union = new Rect(.5f, .5f, 0, 0);
        if (frames != null)
            foreach (int id in frames)
            {
                // Empty editor frame slots are not white, full-size animation frames.
                if (id == 0 || id == GlobalDefaultHelper.DefaultTexId ||
                    !TexAssetForm.DataById.TryGetValue(id, out var data))
                    continue;
                var animation = data.GetAnimationFrames();
                if (animation != null && animation.Count > 0)
                {
                    foreach (var frame in animation)
                        IncludeTextureBounds(frame.texture, cache, ref union, ref hasTexture, ref hasPixels);
                }
                else
                    IncludeTextureBounds(data.GetTex() as Texture2D, cache, ref union, ref hasTexture, ref hasPixels);
            }
        // Without an imported source keep the authored Collider. Fully transparent
        // imported sources (or sources wholly below the alpha threshold) instead
        // have no horizontal physical/Trigger footprint.
        return hasTexture ? union : new Rect(0, 0, 1, 1);
    }

    // Story appearance previews have no scene-generated GameTex variants yet.
    // Assemble the isolated (mask 0) tile with the very same quarter mapping.
    private static Rect GetOpaqueWangTileBounds(List<int> frames, int mask)
    {
        bool hasTexture = false, hasPixels = false;
        var union = new Rect(.5f, .5f, 0, 0);
        var cache = new Dictionary<Texture2D, Rect>();
        foreach (int id in frames)
        {
            if (id == 0 || id == GlobalDefaultHelper.DefaultTexId || !TexAssetForm.DataById.TryGetValue(id, out var data))
                continue;
            var animation = data.GetAnimationFrames();
            var sources = animation != null && animation.Count > 0
                ? animation.Select(frame => frame.texture) : new[] { data.GetTex() as Texture2D };
            foreach (var source in sources)
            {
                if (source == null) continue;
                Dictionary<int, Sprite> sprites = null;
                try
                {
                    sprites = TileHelper.GetAutoTileSprites(source);
                    IncludeTextureBounds(sprites[mask].texture, cache, ref union, ref hasTexture, ref hasPixels);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Cannot assemble collision texture bounds: " + exception.Message);
                    hasTexture = hasPixels = true;
                    union = new Rect(0, 0, 1, 1);
                }
                finally
                {
                    if (sprites != null)
                        foreach (var sprite in new HashSet<Sprite>(sprites.Values))
                        {
                            UnityEngine.Object.Destroy(sprite.texture);
                            UnityEngine.Object.Destroy(sprite);
                        }
                }
            }
        }
        return hasTexture ? union : new Rect(0, 0, 1, 1);
    }

    internal static void ApplyFirstPartCollisionBounds(GameObject instance, GameObject prefab, Rect bounds, float colliderScale,
        bool centerCollider = true)
    {
        if (instance.transform.childCount == 0 || prefab.transform.childCount == 0)
            return;
        var sourceColliders = prefab.GetComponentsInChildren<Collider>(true)
            .Where(c => c.enabled && (c is BoxCollider || c is SphereCollider)).ToArray();
        var targetColliders = instance.GetComponentsInChildren<Collider>(true)
            .Where(c => c.enabled && (c is BoxCollider || c is SphereCollider)).ToArray();
        // Restore before fitting/anchoring; never accumulate offsets on pool reuse.
        for (int i = 0; i < sourceColliders.Length && i < targetColliders.Length; i++)
        {
            targetColliders[i].transform.localPosition = sourceColliders[i].transform.localPosition;
            targetColliders[i].transform.localScale = sourceColliders[i].transform.localScale;
        }
        var sources = prefab.transform.GetChild(0).GetComponentsInChildren<BoxCollider>(true).Where(c => c.enabled).ToArray();
        var targets = instance.transform.GetChild(0).GetComponentsInChildren<BoxCollider>(true).Where(c => c.enabled).ToArray();
        for (int i = 0; i < sources.Length && i < targets.Length; i++)
        {
            MapUtilController.GetHorizontalBoundsBox(sources[i], bounds, colliderScale, out var size, out var center);
            targets[i].transform.localScale = sources[i].transform.localScale;
            targets[i].size = size;
            targets[i].center = center;
        }
        if (!centerCollider)
        {
            Vector3 offset = MapManager.instance.utilCtrl.GetObjectBackColliderOffset(prefab, bounds, colliderScale,
                instance.transform.lossyScale);
            var shifted = new HashSet<Transform>();
            foreach (var collider in targetColliders)
                shifted.Add(collider.transform);
            foreach (var transform in shifted)
            {
                bool hasShiftedParent = false;
                for (var parent = transform.parent; parent != instance.transform && parent != null; parent = parent.parent)
                    if (shifted.Contains(parent))
                    {
                        hasShiftedParent = true;
                        break;
                    }
                if (!hasShiftedParent && transform.parent != null)
                    transform.localPosition += transform.parent.InverseTransformVector(instance.transform.rotation * offset);
            }
        }
    }

    private static void IncludeTextureBounds(Texture2D texture, Dictionary<Texture2D, Rect> cache,
        ref Rect union, ref bool hasTexture, ref bool hasPixels)
    {
        if (texture == null)
            return;
        hasTexture = true;
        if (!cache.TryGetValue(texture, out var bounds))
        {
            Texture2D readable = null;
            try
            {
                readable = texture.isReadable ? texture : TextureHelper.DeCompress(texture);
                var pixels = readable.GetPixels32();
                int minX = texture.width, minY = texture.height, maxX = -1, maxY = -1;
                for (int y = 0; y < texture.height; y++)
                    for (int x = 0; x < texture.width; x++)
                    {
                        // Ignore nearly transparent speckles; for 8-bit alpha,
                        // >= 0.3 means 77..255 (76 / 255 is still below 0.3).
                        if (pixels[y * texture.width + x].a < CollisionBoundsAlphaThreshold * byte.MaxValue)
                            continue;
                        minX = Mathf.Min(minX, x);
                        minY = Mathf.Min(minY, y);
                        maxX = Mathf.Max(maxX, x);
                        maxY = Mathf.Max(maxY, y);
                    }
                bounds = maxX < 0 ? new Rect(.5f, .5f, 0, 0) : Rect.MinMaxRect(
                    (float)minX / texture.width, (float)minY / texture.height,
                    (float)(maxX + 1) / texture.width, (float)(maxY + 1) / texture.height);
            }
            catch (UnityException exception)
            {
                Debug.LogWarning("Cannot read collision texture bounds: " + exception.Message);
                bounds = new Rect(0, 0, 1, 1);
            }
            finally
            {
                if (readable != null && readable != texture)
                    UnityEngine.Object.Destroy(readable);
            }
            cache.Add(texture, bounds);
        }
        if (bounds.width <= 0 || bounds.height <= 0)
            return;
        union = hasPixels ? Rect.MinMaxRect(Mathf.Min(union.xMin, bounds.xMin), Mathf.Min(union.yMin, bounds.yMin),
            Mathf.Max(union.xMax, bounds.xMax), Mathf.Max(union.yMax, bounds.yMax)) : bounds;
        hasPixels = true;
    }

    private static void FitHorizontalColliders(GameObject part, Rect bounds, float colliderScale)
    {
        var colliders = part.GetComponentsInChildren<Collider>(true)
            .Where(c => c.enabled && (c is BoxCollider || c is SphereCollider))
            .Select(c => (collider: c, trigger: c.isTrigger, transform: c.transform, type: c.GetType(),
                size: c is BoxCollider box ? box.size : Vector3.one * (((SphereCollider)c).radius * 2),
                center: c is BoxCollider boxCenter ? boxCenter.center : ((SphereCollider)c).center)).ToArray();
        foreach (var geometry in colliders)
        {
            var basis = geometry;
            if (geometry.trigger)
                foreach (var body in colliders)
                    if (!body.trigger && body.transform == geometry.transform &&
                        body.center == geometry.center && body.type == geometry.type)
                    {
                        basis = body;
                        break;
                    }
            // Preview generation, live refresh and off-screen meshes all use
            // the same Y/Z diagonal mapping, based on untouched body geometry.
            MapUtilController.GetTextureBoundsBox(basis.size, geometry.size, geometry.center, bounds,
                colliderScale, geometry.transform.localScale.y, out var size, out var center);
            if (geometry.collider is BoxCollider box)
            {
                box.size = size;
                box.center = center;
            }
            else if (geometry.collider is SphereCollider sphere)
            {
                // A sphere cannot independently fit the image's X and Y/Z spans.
                var fitted = sphere.gameObject.AddComponent<BoxCollider>();
                fitted.size = size;
                fitted.center = center;
                fitted.isTrigger = sphere.isTrigger;
                fitted.sharedMaterial = sphere.sharedMaterial;
                fitted.contactOffset = sphere.contactOffset;
                sphere.enabled = false;
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(sphere);
                else
                    UnityEngine.Object.DestroyImmediate(sphere);
            }
        }
    }

    private GameObject CombineNewGoByPrefabs(string name, List<int> prefabKeys, List<int> texRealId, List<Vector3> poss, List<Vector3> scales, List<bool> showShadow)
    {
        var res = new GameObject(name);
        for (int i = 0; i < prefabKeys.Count; i++)
        {
            Debug.Log(name + " " + prefabKeys[i]);
            var go = GameObject.Instantiate(GameObjectAssetForm.DataById[prefabKeys[i]].GetGo(), res.transform);
            go.transform.localPosition = poss[i] + Vector3.up / 2;//̧
            go.transform.localScale = scales[i];

            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            var render = go.GetComponentInChildren<Renderer>();
            render.GetPropertyBlock(propBlock);
            if (texRealId[i] != 0 && !TexAssetForm.DataById.ContainsKey(texRealId[i]))
            {
                Debug.LogError(name + " miss tex " + texRealId[i]);
            }
            if (texRealId[i] == 0 || texRealId[i] == emptyTexId || !TexAssetForm.DataById.ContainsKey(texRealId[i]))
            {
                propBlock.SetTexture("_Tex", Texture2D.whiteTexture);
                if (showShadow[i])
                {
                    render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
                else
                {
                    propBlock.SetFloat("_Show", 0);
                }
            }
            else
            {
                var tex = TexAssetForm.DataById[texRealId[i]].GetTex();
                render.shadowCastingMode = showShadow[i] ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                propBlock.SetTexture("_Tex", tex);
            }
            render.SetPropertyBlock(propBlock);

            foreach (var com in go.GetComponentsInChildren<Collider>())
            {
                var scale = com.transform.lossyScale;
                if (com is BoxCollider box)
                {
                    var trigger = com.gameObject.AddComponent<BoxCollider>();
                    trigger.size = box.size + Graph.ElementwiseDivide(Vector3.one * 0.02f, scale);
                    trigger.center = box.center;
                    trigger.isTrigger = true;
                }
                else if (com is SphereCollider sphere)
                {
                    var trigger = com.gameObject.AddComponent<SphereCollider>();
                    trigger.radius = sphere.radius + 0.02f * Mathf.Max(scale.x, scale.y, scale.z);
                    trigger.center = sphere.center;
                    trigger.isTrigger = true;
                }
            }
        }
        res.SetActive(false);

        return res;
    }


}
