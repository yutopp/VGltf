//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
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

        public IndexedResource<Texture> Export(Texture tex)
        {
            return Context.Resources.Textures.GetOrCall(tex, () => {
                return ForceExport(tex);
            });
        }

        public IndexedResource<Texture> ForceExport(Texture tex)
        {
            var imageIndex = ForceExportPngImage(tex);

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

        public int ForceExportPngImage(Texture tex, bool isLinear = false)
        {
            byte[] pngBytes;

            RenderTexture previous = RenderTexture.active;

            Texture2D readableTex = null;
            RenderTexture renderTex = RenderTexture.GetTemporary(
                tex.width,
                tex.height,
                0,
                RenderTextureFormat.Default,
                isLinear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
            try
            {
                Graphics.Blit(tex, renderTex);

                RenderTexture.active = renderTex;

                readableTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, true, isLinear);
                readableTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                readableTex.Apply();

                pngBytes = readableTex.EncodeToPNG();
            }
            finally
            {
                RenderTexture.active = previous;

                RenderTexture.ReleaseTemporary(renderTex);
                Utils.Destroy(readableTex);
            }

            var viewIndex = Context.BufferBuilder.AddView(new ArraySegment<byte>(pngBytes));

            return Context.Gltf.AddImage(new Types.Image
            {
                Name = tex.name,

                MimeType = Types.Image.MimeTypeEnum.ImagePng,
                BufferView = viewIndex,
            });
        }
    }
}
