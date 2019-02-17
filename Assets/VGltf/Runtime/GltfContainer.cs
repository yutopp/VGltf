//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using VJson;

namespace VGltf
{
    public class GltfContainer
    {
        public Types.Gltf Gltf { get; private set; }
        public object Storage { get; private set; }

        public GltfContainer(Types.Gltf gltf, object storage)
        {
            Gltf = gltf;
            Storage = storage;
        }

        public static GltfContainer FromGlb(Stream s)
        {
            return Glb.Reader.ReadAsContainer(s);
        }
    }
}
