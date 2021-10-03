//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VGltf.Unity
{
    public abstract class TextureImporterHook
    {
        public virtual void PostHook(TextureImporter importer)
        {
        }
    }

    public sealed class TextureImporter : ImporterRefHookable<TextureImporterHook>
    {
        public override IImporterContext Context { get; }

        public TextureImporter(IImporterContext context)
        {
            Context = context;
        }

        public async Task<IndexedResource<Texture2D>> Import(int texIndex, CancellationToken ct)
        {
            var gltf = Context.Container.Gltf;

            return await Context.Resources.Textures.GetOrCallAsync(texIndex, async () =>
            {
                return await ForceImport(texIndex, ct);
            });
        }

        public async Task<IndexedResource<Texture2D>> ForceImport(int texIndex, CancellationToken ct)
        {
            var gltf = Context.Container.Gltf;
            var gltfTex = gltf.Textures[texIndex];

            var tex = new Texture2D(0, 0);
            tex.name = gltfTex.Name;

            var resource = Context.Resources.Textures.Add(texIndex, texIndex, tex.name, tex);

            if (gltfTex.Source != null)
            {
                var imageResource = Context.Importers.Images.Import(gltfTex.Source.Value);

                var imageBuffer = new byte[imageResource.Data.Count];
                Array.Copy(imageResource.Data.Array, imageResource.Data.Offset, imageBuffer, 0, imageResource.Data.Count);

                // offload texture decoding...
                tex.LoadImage(imageBuffer);
            }

            return resource;
        }
    }
}
