//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

namespace VGltf.Unity
{
    public partial class ImporterBase
    {
        public GltfContainer Container { get; }
        public ResourcesCache Cache { get; }
        public ResourcesStore BufferView { get; }

        public ImporterBase(GltfContainer container, ResourcesCache resCache, ResourcesStore bufView)
        {
            Container = container;
            Cache = resCache;
            BufferView = bufView;
        }

        public ImporterBase(GltfContainer container, ResourcesCache resCache, IResourceLoader loader)
            : this(container, resCache, new ResourcesStore(container.Gltf, container.Buffer, loader))
        {
        }

        public ImporterBase(ImporterBase b)
            : this(b.Container, b.Cache, b.BufferView)
        {
        }
    }
}
