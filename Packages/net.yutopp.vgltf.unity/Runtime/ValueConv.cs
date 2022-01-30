//
// Copyright (c) 2022- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using UnityEngine;

namespace VGltf.Unity
{
    public static class ValueConv
    {
        // IMPORTANT: We assume that Linear workflow is used.
        // Unity(Color[sRGB]) | Shader(sRGB->Linear)
        // Unity(HDR Color[Linear]) | Shader(Linear)

        // glTF world(sRGB) -> as is
        public static Color ColorFromSRGB(Vector4 v)
        {
            return v;
        }

        // glTF world(sRGB) -> sRGB
        public static Color ColorFromLinear(Vector3 v)
        {
            return ColorFromLinear(new Vector4(v.x, v.y, v.z, 1.0f));
        }

        // glTF world(Linear) -> sRGB
        public static Color ColorFromLinear(Vector4 v)
        {
            return ((Color)v).gamma;
        }

        // Unity(Color[sRGB]) -> Linear
        public static Color ColorToLinear(Color c)
        {
            return c.linear;
        }

        // Unity(Color[sRGB]) -> Linear
        public static Vector3 ColorToLinearRGB(Color c)
        {
            var l = ColorToLinear(c);
            return new Vector3(l.r, l.g, l.b);
        }

        // ---

        // glTF:  roughness : 0 -> 1 (rough)
        // Unity: smoothness: 0 -> 1 (smooth)
        // https://blog.unity.com/ja/technology/ggx-in-unity-5-3
        // roughness = (1 - smoothness) ^ 2
        public static float SmoothnessToRoughness(float glossiness)
        {
            return Mathf.Pow(1.0f - glossiness, 2);
        }

        public static float RoughnessToSmoothness(float roughness)
        {
            return 1.0f - Mathf.Sqrt(roughness);
        }

        // ---

        // https://github.com/KhronosGroup/glTF/issues/1593
        // glTF (sRGB)
        //  R: AO is always sampled from the red channel
        //  G: [unused]
        //  B: [unused]
        //  A: [ignored]

        // https://catlikecoding.com/unity/tutorials/rendering/part-10/
        // Unity (sRGB)
        //  R: [unused]
        //  G: Unity's standard shader uses the G color channel of the occlusion map
        //  B: [unused]
        //  A: [ignored]
        public static Color ConvertGltfOcclusionPixelToUnity(Color c)
        {
            return new Color(0.0f, c.r, 0.0f, 1.0f);
        }

        public static Color ConvertUnityOcclusionPixelToGltf(Color c)
        {
            return new Color(c.g, 0.0f, 0.0f, 1.0f);
        }

        // ---

        // https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#metallic-roughness-material
        // glTF (linear)
        //  R: [unused]
        //  G: roughness
        //  B: metalness
        //  A: [unused]

        // https://docs.unity3d.com/Manual/StandardShaderMaterialParameterMetallic.html
        // Unity (linear)
        //  R: Metalic
        //  G: [unused]
        //  B: [unused]
        //  A: Smoothness (Gloss)
        public static Color RoughnessPixelToGlossPixel(Color c, float metallic, float roughness)
        {
            return new Color(
                c.b * metallic,
                0.0f,
                0.0f,
                RoughnessToSmoothness(c.g * roughness)
                );
        }

        public static Color GlossPixelToRoughnessPixel(Color c, float metallic, float smoothness)
        {
            return new Color(
                0.0f,
                SmoothnessToRoughness(c.a * smoothness),
                c.r * metallic,
                1.0f
                );
        }
    }
}
