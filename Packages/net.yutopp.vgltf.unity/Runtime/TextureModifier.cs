//
// Copyright (c) 2022- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Threading.Tasks;
using UnityEngine;

namespace VGltf.Unity
{
    public static class TextureModifier
    {
        // TODO: non-blocking version
        // Unity -> glTF
        public static void OverwriteUnityDXT5nmNormalTexToGltf(Texture2D tex)
        {
            var pixels = tex.GetPixels();
            for (var i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = ValueConv.ConvertUnityDXT5nmNormalTexToGltf(pixels[i]);
            }
            tex.SetPixels(pixels);
        }

        // --

        // TODO: non-blocking version
        // Unity -> glTF
        public static void OverwriteUnityOcclusionTexToGltf(Texture2D tex)
        {
            var pixels = tex.GetPixels();
            for (var i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = ValueConv.ConvertUnityOcclusionPixelToGltf(pixels[i]);
            }
            tex.SetPixels(pixels);
        }

        // --

        // TODO: non-blocking version
        // Unity -> glTF
        public static void OverriteToGlossMapToRoughnessMap(Texture2D tex, float metallic, float smoothness)
        {
            var pixels = tex.GetPixels();
            for (var i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = ValueConv.GlossPixelToRoughnessPixel(pixels[i], metallic, smoothness);
            }
            tex.SetPixels(pixels);
        }
    }
}
