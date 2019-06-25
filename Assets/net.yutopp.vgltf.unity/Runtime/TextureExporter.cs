//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using UnityEngine;

namespace VGltf.Unity
{
    public class TextureExporter : ExporterBase
    {
        public TextureExporter(ExporterBase parent)
            : base(parent)
        {
        }

        public IndexedResource<Texture2D> Export(Texture2D tex)
        {
            return Cache.CacheObjectIfNotExists(tex.name, tex, Cache.Textures, ForceExport);
        }

        public IndexedResource<Texture2D> ForceExport(Texture2D tex)
        {
            var imageExporter = new ImageExporter(this);

            var imageIndex = imageExporter.Export(tex);

            var gltfImage = new Types.Texture
            {
                Name = tex.name,

                //Sampler = primitives,
                Source = imageIndex,
            };
            return new IndexedResource<Texture2D>
            {
                Index = Types.GltfExtensions.AddTexture(Gltf, gltfImage),
                Value = tex,
            };
        }
    }
}
