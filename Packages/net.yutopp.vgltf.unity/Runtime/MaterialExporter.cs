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
                    MetallicFactor = 0.0f,  // TODO: fix
                    RoughnessFactor = 1.0f, // TODO: fix
                },
            };

            var matIndex = Context.Gltf.AddMaterial(gltfMaterial);
            var resource = Context.Resources.Materials.Add(mat, matIndex, mat.name, mat);

            return resource;
        }
    }
}
