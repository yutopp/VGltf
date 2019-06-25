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

        public ImporterBase(GltfContainer container, ResourcesCache resCache)
        {
            Container = container;
            Cache = resCache;
        }

        public ImporterBase(ImporterBase b)
            : this(b.Container, b.Cache)
        {
        }
    }
}
