//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using UnityEngine;
using VGltf.Types.Extensions;

namespace VGltf.Unity
{
    public abstract class MaterialExporterHook
    {
        public abstract IndexedResource<Material> Export(IExporterContext context, Material mat);
    }

    public class MaterialExporter : ExporterRefHookable<MaterialExporterHook>
    {
        public override IExporterContext Context { get; }

        public MaterialExporter(IExporterContext context)
        {
            Context = context;
        }

        public IndexedResource<Material> Export(Material mat)
        {
            return Context.Resources.Materials.GetOrCall(mat, () =>
            {
                return ForceExport(mat);
            });
        }

        public IndexedResource<Material> ForceExport(Material mat)
        {
            foreach (var h in Hooks)
            {
                var r = h.Export(Context, mat);
                if (r != null)
                {
                    return r;
                }
            }

            // TODO: Support various shaders
            switch (mat.shader.name)
            {
                case "Unlit/Texture":
                    return ForceExportUnlit(mat);
                default:
                    return ForceExportStandard(mat);
            }
        }

        public IndexedResource<Material> ForceExportUnlit(Material mat)
        {
            var tex = mat.GetTexture("_MainTex");
            IndexedResource<Texture> textureResource = null;
            if (tex != null)
            {
                textureResource = Context.Exporters.Textures.Export(tex);
            }

            var gltfMaterial = new Types.Material
            {
                Name = mat.name,

                PbrMetallicRoughness = new Types.Material.PbrMetallicRoughnessType
                {
                    BaseColorTexture = textureResource != null ? new Types.Material.BaseColorTextureInfoType
                    {
                        Index = textureResource.Index,
                        TexCoord = 0, // NOTE: mesh.primitive must have TEXCOORD_<TexCoord>.
                    } : null, // TODO: fix
                    BaseColorFactor = new float[] { 1.0f, 1.0f, 1.0f, 1.0f }, // while defaultly
                },
            };
            gltfMaterial.AddExtension(
                VGltf.Ext.KhrMaterialsUnlit.Types.KhrMaterialsUnlit.ExtensionName,
                new VGltf.Ext.KhrMaterialsUnlit.Types.KhrMaterialsUnlit { }
                );

            var matIndex = Context.Gltf.AddMaterial(gltfMaterial);
            var resource = Context.Resources.Materials.Add(mat, matIndex, mat.name, mat);

            // Mark an extension as used
            Context.Gltf.AddExtensionUsed(VGltf.Ext.KhrMaterialsUnlit.Types.KhrMaterialsUnlit.ExtensionName);

            return resource;
        }

        public IndexedResource<Material> ForceExportStandard(Material mat)
        {
            // Maybe, Standard shader...
            // TODO: Support various shaders

            var mainColor = mat.GetColor("_Color");
            var mainTex = ExportTextureIfExist(mat, "_MainTex");

            var metallic = mat.GetFloat("_Metallic");
            var roughness = ValueConv.SmoothnessToRoughness(mat.GetFloat("_Glossiness"));

            var normalMap = ExportTextureIfExist(mat, "_BumpMap", true);

            var emissionColor = mat.GetColor("_EmissionColor");
            var emissionTex = ExportTextureIfExist(mat, "_EmissionMap");

            var gltfMaterial = new Types.Material
            {
                Name = mat.name,

                PbrMetallicRoughness = new Types.Material.PbrMetallicRoughnessType
                {
                    BaseColorFactor = PrimitiveExporter.AsArray(ValueConv.ColorToLinear(mainColor)),
                    BaseColorTexture = mainTex != null ? new Types.Material.BaseColorTextureInfoType
                    {
                        Index = mainTex.Index,
                        TexCoord = 0, // NOTE: mesh.primitive must have TEXCOORD_<TexCoord>.
                    } : null, // TODO: fix
                    MetallicFactor = metallic,
                    RoughnessFactor = roughness,
                    // MetallicRoughnessTexture
                },

                NormalTexture = normalMap != null ? new Types.Material.NormalTextureInfoType
                {
                    Index = normalMap.Index,
                    TexCoord = 0, // NOTE: mesh.primitive must have TEXCOORD_<TexCoord>.
                } : null,

                // OcclusionTexture

                EmissiveFactor = emissionColor != Color.black
                   ? PrimitiveExporter.AsArray(ValueConv.ColorToLinearRGB(emissionColor))
                   : null,
                EmissiveTexture = emissionTex != null ? new Types.Material.EmissiveTextureInfoType
                {
                    Index = emissionTex.Index,
                    TexCoord = 0, // NOTE: mesh.primitive must have TEXCOORD_<TexCoord>.
                } : null, // TODO: fix

                AlphaMode = GetAlphaMode(mat),
                // DoubleSided = // Not supported
            };

            var matIndex = Context.Gltf.AddMaterial(gltfMaterial);
            var resource = Context.Resources.Materials.Add(mat, matIndex, mat.name, mat);

            return resource;
        }

        Types.Material.AlphaModeEnum GetAlphaMode(Material mat)
        {
            var modeValue = mat.GetFloat("_Mode");
            if (modeValue == 0) return Types.Material.AlphaModeEnum.Opaque;
            else if (modeValue == 1) return Types.Material.AlphaModeEnum.Mask;
            else if (modeValue == 2 || modeValue == 3) return Types.Material.AlphaModeEnum.Blend; // TODO: Fade support
            else return Types.Material.AlphaModeEnum.Opaque; // fallback
        }

        IndexedResource<Texture> ExportTextureIfExist(Material mat, string name, bool isLinear = false)
        {
            var res = default(IndexedResource<Texture>);
            var tex = mat.GetTexture(name);
            if (tex != null)
            {
                res = Context.Exporters.Textures.Export(tex, isLinear);
            }

            return res;
        }
    }
}
