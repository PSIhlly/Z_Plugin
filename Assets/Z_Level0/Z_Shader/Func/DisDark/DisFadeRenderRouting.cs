using System.Collections.Generic;
using UnityEngine;

/// <summary>DisFade-only source/texture/pass materials; MPBs contain instanced scalars only.
/// Callers guarantee the shader; only null/multi-material boundaries are checked.</summary>
public static class DisFadeRenderRouting
{
    public enum Route { Hidden, Opaque, Transparent, Mixed }
    // Material pass switches match LightMode tags; FindPass uses the shader's Name instead.
    private const string ForwardPass = "UniversalForward", OpaquePass = "DisFadeOpaque";
    private static readonly int Alpha = Shader.PropertyToID("_Alpha"), Show = Shader.PropertyToID("_Show"),
        FadeCenter = Shader.PropertyToID("_FadeCenter"), Cutoff = Shader.PropertyToID("_Cutoff"),
        LightSensitivity = Shader.PropertyToID("_LightSensitivity"), MainTexture = Shader.PropertyToID("_Tex");
    private static readonly Dictionary<Material, SourceMaterial> sources = new Dictionary<Material, SourceMaterial>();
    private static readonly Dictionary<Material, MaterialVariants> materialOwners = new Dictionary<Material, MaterialVariants>();
    private static bool splitCamera;

    // One property snapshot per original material, shared by all texture variants.
    // Visibility and texture changes reuse these defaults without recapturing
    // Material properties on a warm cache hit.
    private sealed class SourceMaterial
    {
        public readonly Material material;
        public readonly Dictionary<Texture, MaterialVariants> variants = new Dictionary<Texture, MaterialVariants>();
        public float alpha, show, fade, cutoff, light;
        public int renderQueue;
        public bool forwardEnabled, opaqueEnabled;
        public int version { get; private set; }

        public SourceMaterial(Material material)
        {
            this.material = material;
            Capture();
        }

        private void Capture()
        {
            alpha = material.GetFloat(Alpha);
            show = material.GetFloat(Show);
            fade = material.GetFloat(FadeCenter);
            cutoff = material.GetFloat(Cutoff);
            light = material.GetFloat(LightSensitivity);
            renderQueue = material.renderQueue;
            forwardEnabled = material.GetShaderPassEnabled(ForwardPass);
            opaqueEnabled = material.GetShaderPassEnabled(OpaquePass);
            version++;
        }

        public Material Get(Texture texture, Route route)
        {
            texture = NormalizeTexture(texture);
            if (!variants.TryGetValue(texture, out var value))
            {
                value = new MaterialVariants(this, texture);
                variants.Add(texture, value);
            }
            return value.Get(route);
        }
    }

    private sealed class MaterialVariants
    {
        private readonly SourceMaterial sourceData;
        public SourceMaterial sourceInfo => sourceData;
        public Material source => sourceData.material;
        public readonly Texture texture;
        public Material hidden, mixed, opaque, transparent;
        public MaterialVariants(SourceMaterial source, Texture texture) { sourceData = source; this.texture = texture; }

        public Material Get(Route route)
        {
            switch (route)
            {
                case Route.Hidden:
                    if (hidden == null) hidden = Create("Hidden", route);
                    return hidden;
                case Route.Opaque:
                    if (opaque == null) opaque = Create("Opaque", route);
                    return opaque;
                case Route.Transparent:
                    if (transparent == null) transparent = Create("Transparent", route);
                    return transparent;
                default:
                    if (mixed == null) mixed = Create("Mixed", route);
                    return mixed;
            }
        }
        private Material Create(string suffix, Route route)
        {
            var material = new Material(source) {
                name = source.name + " (DisFade " + suffix + ")", hideFlags = HideFlags.HideAndDontSave };
            Configure(material, route);
            // Bind once during initialization, before publishing the material.
            // Later texture changes select another cached MaterialVariants entry.
            material.SetTexture(MainTexture, texture);
            materialOwners.Add(material, this);
            return material;
        }
        private void Configure(Material material, Route route)
        {
            material.enableInstancing = true;
            material.renderQueue = sourceData.renderQueue;
            material.SetFloat(Alpha, sourceData.alpha);
            material.SetFloat(Show, sourceData.show);
            material.SetFloat(FadeCenter, sourceData.fade);
            material.SetFloat(Cutoff, sourceData.cutoff);
            material.SetFloat(LightSensitivity, sourceData.light);
            // Hidden skips color draws while retaining the source's ShadowCaster pass.
            material.SetShaderPassEnabled(ForwardPass, sourceData.forwardEnabled && route != Route.Hidden
                && (route != Route.Opaque || !splitCamera));
            material.SetShaderPassEnabled(OpaquePass, sourceData.opaqueEnabled
                && route != Route.Transparent && route != Route.Hidden);
        }
      
        public void SetSplitCamera(bool active)
        {
            if (opaque != null)
                opaque.SetShaderPassEnabled(ForwardPass, sourceData.forwardEnabled && !active);
        }
    }






   

    /// <summary>Assigns a material bound to this texture; never changes an existing material's texture.</summary>
    public static void SetTexture(Renderer renderer, Texture texture, Route route)
    {
        if (renderer == null || renderer.sharedMaterial == null) return;
        Assign(renderer, GetSource(renderer.sharedMaterial), texture, route);
    }

    /// <summary>Submits scalar overrides and uses the route chosen by the caller.</summary>
    public static void Apply(Renderer renderer, MaterialPropertyBlock block, Route route)
    {
        if (renderer == null) return;
        renderer.SetPropertyBlock(block);
        var material = renderer.sharedMaterial;
        if (material == null) return;
        Texture texture = materialOwners.TryGetValue(material, out var owner)
            ? owner.texture : material.GetTexture(MainTexture);
        Assign(renderer, GetSource(material), texture, route);
    }

    private static void Assign(Renderer renderer, SourceMaterial source, Texture texture, Route route)
    {
        Material material = source.Get(texture, route);
        if (renderer.sharedMaterial != material) renderer.sharedMaterial = material;
    }

    
    private static Texture NormalizeTexture(Texture texture) => texture != null ? texture : Texture2D.whiteTexture;
    internal static Material GetMaterial(Material source, Texture texture, Route route)
    {
        return GetSource(source).Get(texture, route);
    }
    private static SourceMaterial GetSource(Material source)
    {
        // A texture/frame change must never capture an already-routed variant
        // as a new source (its pass flags may be camera-dependent).
        if (materialOwners.TryGetValue(source, out var owner)) return owner.sourceInfo;
        if (!sources.TryGetValue(source, out var value))
        {
            value = new SourceMaterial(source);
            sources.Add(source, value);
        }
        return value;
    }

    public static void BeginSplitCamera()
    {
        splitCamera = true;
        foreach (var source in sources.Values)
        foreach (var value in source.variants.Values)
            value.SetSplitCamera(true);
    }
    public static void EndSplitCamera()
    {
        splitCamera = false;
        foreach (var source in sources.Values)
        foreach (var value in source.variants.Values)
            value.SetSplitCamera(false);
    }

    private static void DestroyVariant(Material material)
    {
        if (material == null) return;
        if (Application.isPlaying) Object.Destroy(material); else Object.DestroyImmediate(material);
    }
}
