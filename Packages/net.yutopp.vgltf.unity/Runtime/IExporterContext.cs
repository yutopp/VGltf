//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

namespace VGltf.Unity
{
    public interface IExporterContext
    {
        Types.Gltf Gltf { get; }
        ExporterRuntimeResources RuntimeResources { get; }
        BufferBuilder BufferBuilder { get; }

        NodeExporter Nodes { get; }
        MeshExporter Meshes { get; }
        MaterialExporter Materials { get; }
        TextureExporter Textures { get; }
        ImageExporter Images { get; }
    }
}
