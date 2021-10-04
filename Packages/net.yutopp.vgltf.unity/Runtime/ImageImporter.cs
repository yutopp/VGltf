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
    public abstract class ImageImporterHook
    {
        public virtual void PostHook(ImageImporter importer)
        {
        }
    }

    public class ImageImporter : ImporterRefHookable<ImageImporterHook>
    {
        public override IImporterContext Context { get; }

        public ImageImporter(IImporterContext context)
        {
            Context = context;
        }

        // TODO: fix interface to check condition of linier/sRGB at here
        public Resource Import(int imgIndex)
        {
            var gltfImgResource = Context.GltfResources.GetOrLoadImageResourceAt(imgIndex);

            return gltfImgResource;
        }
    }
}
