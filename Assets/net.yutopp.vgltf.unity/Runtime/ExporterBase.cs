//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

namespace VGltf.Unity
{
    public partial class ExporterBase
    {
        public Types.Gltf Gltf { get; }
        public ResourcesCache Cache { get; }
        public BufferBuilder BufferBuilder { get; }

        public ExporterBase(Types.Gltf gltf, ResourcesCache resCache, BufferBuilder bufBuilder)
        {
            Gltf = gltf;
            Cache = resCache;
            BufferBuilder = bufBuilder;
        }

        public ExporterBase(ExporterBase b)
            : this(b.Gltf, b.Cache, b.BufferBuilder)
        {
        }
    }
}
