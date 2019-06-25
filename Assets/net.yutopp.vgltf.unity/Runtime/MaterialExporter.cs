//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using UnityEngine;

namespace VGltf.Unity
{
    public class MaterialExporter : ExporterBase
    {
        public MaterialExporter(ExporterBase parent)
            : base(parent)
        {
        }

        public IndexedResource<Material> Export(Material m)
        {
            return Cache.CacheObjectIfNotExists(m.name, m, Cache.Materials, ForceExport);
        }

        public IndexedResource<Material> ForceExport(Material m)
        {
            var textureExporter = new TextureExporter(this);

            // TODO: Support various shaders
            {
                var tex = m.GetTexture("_MainTex") as Texture2D;
                IndexedResource<Texture2D> textureResource = null;
                if (tex != null)
                {
                    textureResource = textureExporter.Export(tex);
                }

                var gltfMaterial = new Types.Material
                {
                    Name = m.name,

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
                return new IndexedResource<Material>
                {
                    Index = Types.GltfExtensions.AddMaterial(Gltf, gltfMaterial),
                    Value = m,
                };
            }
        }
    }
}
