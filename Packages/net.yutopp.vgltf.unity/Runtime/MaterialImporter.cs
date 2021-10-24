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

namespace VGltf.Unity
{
    public abstract class MaterialImporterHook
    {
        public abstract Task<IndexedResource<Material>> Import(IImporterContext context, int matIndex, CancellationToken ct);
    }

    public class MaterialImporter : ImporterRefHookable<MaterialImporterHook>
    {
        public override IImporterContext Context { get; }

        public MaterialImporter(IImporterContext context)
        {
            Context = context;
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

            var shader = default(Shader);
            using (Utils.MeasureAndPrintTime($"Shader.Find"))
            {
                shader = Shader.Find("Standard");
            }
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
                    // Not implemented
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
                    mat.renderQueue = 2450;

                    mat.SetFloat("_Cutoff", gltfMat.AlphaCutoff);
                    break;
            }

            var emissionColor = CoordUtils.ColorFromSRGB(PrimitiveImporter.AsVector3(gltfMat.EmissiveFactor));
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

            if (gltfMat.PbrMetallicRoughness != null)
            {
                var pbrMR = gltfMat.PbrMetallicRoughness;

                // baseColorFactor is linear. See: https://github.com/KhronosGroup/glTF/issues/1638
                var baseColor = CoordUtils.ColorFromLinear(PrimitiveImporter.AsVector4(pbrMR.BaseColorFactor));
                mat.SetColor("_Color", baseColor);

                if (pbrMR.BaseColorTexture != null)
                {
                    var textureResource = await Context.Importers.Textures.Import(pbrMR.BaseColorTexture.Index, false, ct);
                    mat.SetTexture("_MainTex", textureResource.Value);
                }
            }
        }
    }
}
