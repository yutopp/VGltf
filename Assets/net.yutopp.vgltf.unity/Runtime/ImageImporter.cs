//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using UnityEngine;

namespace VGltf.Unity
{
    public class ImageImporter : ImporterBase
    {
        public ImageImporter(ImporterBase parent)
            : base(parent)
        {
        }

        public Resource Import(int imgIndex)
        {
            var gltfImgResource = BufferView.GetOrLoadImageResourceAt(imgIndex);

            return gltfImgResource;
        }
    }
}
