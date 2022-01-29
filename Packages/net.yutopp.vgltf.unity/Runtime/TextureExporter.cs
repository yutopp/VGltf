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

        public IndexedResource<Texture> Export(Texture tex, bool isLinear = false)
        {
            return Context.Resources.Textures.GetOrCall(tex, () => {
                return ForceExport(tex, isLinear);
            });
        }

        public IndexedResource<Texture> ForceExport(Texture tex, bool isLinear = false)
        {
            var imageIndex = Context.Exporters.Images.Export(tex, isLinear);

            var gltfImage = new Types.Texture
            {
                Name = tex.name,

                //Sampler = primitives,
                Source = imageIndex,
            };
            var texIndex = Context.Gltf.AddTexture(gltfImage);
            var resource = Context.Resources.Textures.Add(tex, texIndex, tex.name, tex);

            return resource;
        }
    }
}
