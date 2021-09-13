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
    public class ImageExporter : ExporterRefHookable<uint>
    {
        public override IExporterContext Context { get; }

        public ImageExporter(IExporterContext context)
        {
            Context = context;
        }

        public int Export(Texture tex)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                tex.width,
                tex.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(tex, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, true, true);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex); // TODO: fix resource leak when exceptions raised

            var pngBytes = readableText.EncodeToPNG();
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
