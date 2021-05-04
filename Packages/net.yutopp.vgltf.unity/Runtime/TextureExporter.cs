//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using UnityEngine;
using VGltf.Types.Extensions;

namespace VGltf.Unity
{
    public class TextureExporter : ExporterRefHookable<NodeExporterHook>
    {
        public override IExporterContext Context { get; }

        public TextureExporter(IExporterContext context)
        {
            Context = context;
        }

        public IndexedResource<Texture2D> Export(Texture2D tex)
        {
            return Context.RuntimeResources.Textures.GetOrCall(tex.name, () => {
                return ForceExport(tex);
            });
        }

        public IndexedResource<Texture2D> ForceExport(Texture2D tex)
        {
            var imageIndex = Context.Images.Export(tex);

            var gltfImage = new Types.Texture
            {
                Name = tex.name,

                //Sampler = primitives,
                Source = imageIndex,
            };
            var texIndex = Context.Gltf.AddTexture(gltfImage);
            var resource = Context.RuntimeResources.Textures.Add(tex.name, texIndex, tex);

            return resource;
        }
    }
}
