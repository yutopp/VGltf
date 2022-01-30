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
    }
}
