//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace VGltf.Unity
{
    public abstract class MaterialImporterHook
    {
        public abstract Task<IndexedResource<Material>> Import(IImporterContext context, int matIndex, CancellationToken ct);
    }

    public sealed class MaterialImporter : ImporterRefHookable<MaterialImporterHook>
    {
        public sealed class Config
        {
            public bool SkipConvertingNormalTex;
            public bool SkipConvertingOcclusionTex;
            public bool SkipConvertingMetallicRoughness;
        }

        public override IImporterContext Context { get; }
        readonly Config _config;

        public MaterialImporter(IImporterContext context, Config config)
        {
            Context = context;
            _config = config;
        }

        public async Task<IndexedResource<Material>> Import(int matIndex, CancellationToken ct)
        {
            var gltf = Context.Container.Gltf;
            var gltfMat = gltf.Materials[matIndex];

            return await Context.Resources.Materials.GetOrCallAsync(matIndex, async () =>
            {
                return await ForceImport(matIndex, ct);
            });
        }

        public async Task<IndexedResource<Material>> ForceImport(int matIndex, CancellationToken ct)
        {
            foreach (var h in Hooks)
            {
                var r = await h.Import(Context, matIndex, ct);
                if (r != null)
                {
                    return r;
                }
            }

            // Default import
            var gltf = Context.Container.Gltf;
            var gltfMat = gltf.Materials[matIndex];

            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                throw new Exception($"Standard shader is not found");
            }

            var mat = new Material(shader);
            mat.name = gltfMat.Name;

            var resource = Context.Resources.Materials.Add(matIndex, matIndex, mat.name, mat);

            await ImportStandardMaterialProps(mat, gltfMat, ct);

            return resource;
        }

        public async Task ImportStandardMaterialProps(Material mat, Types.Material gltfMat, CancellationToken ct)
        {
            if (gltfMat.DoubleSided)
            {
                // Not supported
            }

            // https://forum.unity.com/threads/standard-material-shader-ignoring-setfloat-property-_mode.344557/
            switch (gltfMat.AlphaMode)
            {
                case Types.Material.AlphaModeEnum.Opaque:
                    mat.SetFloat("_Mode", (float)0);
                    mat.SetOverrideTag("RenderType", "");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = -1;
                    break;

                case Types.Material.AlphaModeEnum.Blend:
                    mat.SetFloat("_Mode", (float)3);
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;

                case Types.Material.AlphaModeEnum.Mask:
                    mat.SetFloat("_Mode", (float)1);
                    mat.SetOverrideTag("RenderType", "TransparentCutout");
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.EnableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

                    mat.SetFloat("_Cutoff", gltfMat.AlphaCutoff);
                    break;
            }

            // RGB component and NOT [HDR]
            var emissionColor = ValueConv.ColorFromLinear(PrimitiveImporter.AsVector3(gltfMat.EmissiveFactor));
            if (emissionColor != Color.black)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor);
            }

            if (gltfMat.EmissiveTexture != null)
            {
                var textureResource = await Context.Importers.Textures.Import(gltfMat.EmissiveTexture.Index, false, ct);
                mat.SetTexture("_EmissionMap", textureResource.Value);
            }

            if (gltfMat.NormalTexture != null)
            {
                mat.EnableKeyword("_NORMALMAP");

                var texture = await NormalTextureImporter.Import(
                    Context,
                    _config.SkipConvertingNormalTex,
                    gltfMat.NormalTexture.Index,
                    ct);
                mat.SetTexture("_BumpMap", texture);
            }

            if (gltfMat.OcclusionTexture != null)
            {
                var texture = await OcclusionTextureImporter.Import(
                    Context,
                    _config.SkipConvertingOcclusionTex,
                    gltfMat.OcclusionTexture.Index,
                    ct);
                mat.SetTexture("_OcclusionMap", texture);

                mat.SetFloat("_OcclusionStrength", gltfMat.OcclusionTexture.Strength);
            }

            if (gltfMat.PbrMetallicRoughness != null)
            {
                var pbrMR = gltfMat.PbrMetallicRoughness;

                // baseColorFactor is linear. See: https://github.com/KhronosGroup/glTF/issues/1638
                var baseColor = ValueConv.ColorFromLinear(PrimitiveImporter.AsVector4(pbrMR.BaseColorFactor));
                mat.SetColor("_Color", baseColor);

                if (pbrMR.BaseColorTexture != null)
                {
                    var textureResource = await Context.Importers.Textures.Import(pbrMR.BaseColorTexture.Index, false, ct);
                    mat.SetTexture("_MainTex", textureResource.Value);
                }

                if (pbrMR.MetallicRoughnessTexture != null)
                {
                    mat.EnableKeyword("_METALLICGLOSSMAP");

                    var texture = await MetallicRoughnessTextureImporter.Import(
                        Context,
                        _config.SkipConvertingMetallicRoughness,
                        pbrMR.MetallicRoughnessTexture.Index,
                        pbrMR.MetallicFactor,
                        pbrMR.RoughnessFactor,
                        ct
                        );
                    mat.SetTexture("_MetallicGlossMap", texture);

                    // Values are already baked into textures, thus set 1.0 to make no effects.
                    mat.SetFloat("_Metallic", 1.0f);
                    mat.SetFloat("_Glossiness", 1.0f);
                }
                else
                {
                    mat.SetFloat("_Metallic", pbrMR.MetallicFactor);
                    mat.SetFloat("_Glossiness", ValueConv.RoughnessToSmoothness(pbrMR.RoughnessFactor));
                }
            }
        }

        public static class NormalTextureImporter
        {
            struct NormalTexKey
            {
                public int Index;
            }

            public static async Task<Texture2D> Import(
                IImporterContext context,
                bool skipConverting,
                int index,
                CancellationToken ct)
            {
                if (skipConverting)
                {
                    var res = await context.Importers.Textures.Import(index, true, ct);
                    return res.Value;
                }

                // NormalMap is not color (= as is (linear))
                // NOTE: If UNITY_NO_DXT5nm is enabled, NO modification is required
                if (GraphicsSettings.HasShaderDefine(BuiltinShaderDefine.UNITY_NO_DXT5nm))
                {
                    var res = await context.Importers.Textures.Import(index, true, ct);
                    return res.Value;
                }
                else
                {
                    var src = await context.Importers.Textures.RawImport(index, true, ct);
                    using (var srcRes = new OverwroteTexDisposable(src))
                    {
                        var texture = await GenerateUnityDXT5nmFromGltfNormal(src);

                        // TODO: support multi-set
                        context.Resources.AuxResources.Add(new NormalTexKey
                        {
                            Index = index,
                        }, new OverwroteTexDisposable(texture));

                        return texture;
                    }
                }
            }

            public static Task<Texture2D> GenerateUnityDXT5nmFromGltfNormal(Texture2D src)
            {
                var dst = new Texture2D(src.width, src.height, TextureFormat.RGBA32 /*TextureFormat.DXT5*/, 0, true);
                try
                {
                    ImageUtils.BlitTex(src, dst, true);
                    TextureModifier.OverwriteGltfNormalTexToUnityDXT5nm(dst);
                    dst.Apply();
                }
                catch
                {
                    Utils.Destroy(dst);
                    throw;
                }

                return Task.FromResult(dst);
            }
        }

        public static class OcclusionTextureImporter
        {
            struct OcclusionTexKey
            {
                public int Index;
            }

            public static async Task<Texture2D> Import(
                IImporterContext context,
                bool skipConverting,
                int index,
                CancellationToken ct)
            {
                if (skipConverting)
                {
                    var res = await context.Importers.Textures.Import(index, false, ct);
                    return res.Value;
                }

                var src = await context.Importers.Textures.RawImport(index, false, ct);
                using (var srcRes = new OverwroteTexDisposable(src))
                {
                    var texture = await GenerateOcclusionFromGltf(src);

                    // TODO: support multi-set
                    context.Resources.AuxResources.Add(new OcclusionTexKey
                    {
                        Index = index,
                    }, new OverwroteTexDisposable(texture));

                    return texture;
                }
            }

            public static Task<Texture2D> GenerateOcclusionFromGltf(Texture2D src)
            {
                // GlossMap uses R
                var dst = new Texture2D(src.width, src.height, TextureFormat.RGBA32 /*TextureFormat.R8*/, 0, false);
                try
                {
                    ImageUtils.BlitTex(src, dst, false);
                    TextureModifier.OverwriteGltfOcclusionTexToUnity(dst);
                    dst.Apply();
                }
                catch
                {
                    Utils.Destroy(dst);
                    throw;
                }

                return Task.FromResult(dst);
            }
        }

        public static class MetallicRoughnessTextureImporter
        {
            struct MetallicRoughnessTexKey
            {
                public int Index;
            }

            public static async Task<Texture2D> Import(
                IImporterContext context,
                bool skipConverting,
                int index,
                float metallic,
                float roughness,
                CancellationToken ct)
            {
                if (skipConverting)
                {
                    var res = await context.Importers.Textures.Import(index, true, ct);
                    return res.Value;
                }

                var src = await context.Importers.Textures.RawImport(index, true, ct);
                using (var srcRes = new OverwroteTexDisposable(src))
                {
                    var texture = await GenerateGlossMapFromGltfRoughnessMap(src, metallic, roughness);

                    // TODO: support multi-set
                    context.Resources.AuxResources.Add(new MetallicRoughnessTexKey
                    {
                        Index = index,
                    }, new OverwroteTexDisposable(texture));

                    return texture;
                }
            }

            public static Task<Texture2D> GenerateGlossMapFromGltfRoughnessMap(Texture2D src, float metallic, float roughness)
            {
                // GlossMap uses R, A
                var dst = new Texture2D(src.width, src.height, TextureFormat.RGBA32, 0, true);
                try
                {
                    ImageUtils.BlitTex(src, dst, true);
                    TextureModifier.OverriteRoughnessMapToGlossMap(dst, metallic, roughness);
                    dst.Apply();
                }
                catch
                {
                    Utils.Destroy(dst);
                    throw;
                }

                return Task.FromResult(dst);
            }
        }

        sealed class OverwroteTexDisposable : IDisposable
        {
            readonly Texture2D _tex;

            public OverwroteTexDisposable(Texture2D tex)
            {
                _tex = tex;
            }

            public void Dispose()
            {
                Utils.Destroy(_tex);
            }
        }
    }
}
