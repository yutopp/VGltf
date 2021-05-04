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
        public abstract IndexedResource<Material> Export(MaterialExporter exporter, Material mat);
    }

    public class MaterialExporter : ExporterRefHookable<MaterialExporterHook>
    {
        public override IExporterContext Context { get; }

        public MaterialExporter(IExporterContext context)
        {
            Context = context;
        }

        public IndexedResource<Material> Export(Material m)
        {
            return Context.RuntimeResources.Materials.GetOrCall(m.name, () =>
            {
                return ForceExport(m);
            });
        }

        public IndexedResource<Material> ForceExport(Material mat)
        {
            foreach (var h in Hooks)
            {
                var r = h.Export(this, mat);
                if (r != null)
                {
                    return r;
                }
            }

            // Maybe, Standard shader...
            // TODO: Support various shaders
            var tex = mat.GetTexture("_MainTex") as Texture2D;
            IndexedResource<Texture2D> textureResource = null;
            if (tex != null)
            {
                textureResource = Context.Textures.Export(tex);
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
            var resource = Context.RuntimeResources.Materials.Add(mat.name, matIndex, mat);

            return resource;
        }
    }
}
