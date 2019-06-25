//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VGltf.Unity
{
    public class CoordUtils
    {
        public static IEnumerable<int> FlipIndices(int[] xs)
        {
            for (int i = 0; i < xs.Length / 3; ++i)
            {
                // From: 0, 1, 2, 3, 4, 5, ...
                // To:   2, 1, 0, 5, 4, 3, ...
                yield return xs[i * 3 + 2];
                yield return xs[i * 3 + 1];
                yield return xs[i * 3 + 0];
            }
        }

        public static Vector2 ConvertUV(Vector2 v)
        {
            // (u, v)
            // (u, 1 - v)
            return new Vector2(v.x, 1 - v.y);
        }

        public static Vector3 ConvertSpace(Vector3 v)
        {
            // ???
            // TODO: fix
            return new Vector3(v.x, v.y, -v.z);
        }

        public static Vector4 ConvertSpace(Vector4 v)
        {
            // ???
            // TODO: fix
            return new Vector4(v.x, v.y, -v.z, -v.w);
        }

        public static Quaternion ConvertSpace(Quaternion v)
        {
            // ???
            // TODO: fix
            return new Quaternion(-v.x, -v.y, v.z, v.w);
        }
    }
}
