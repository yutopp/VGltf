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
    public class ImageExporter : ExporterRef
    {
        public ImageExporter(Exporter parent)
            : base(parent)
        {
        }

        public int Export(Texture2D tex)
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
            RenderTexture.ReleaseTemporary(renderTex);

            var pngBytes = readableText.EncodeToPNG();
            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(pngBytes));

            return Gltf.AddImage(new Types.Image
            {
                Name = tex.name,

                MimeType = Types.Image.MimeTypeEnum.ImagePng,
                BufferView = viewIndex,
            });
        }
    }
}
