//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;

namespace VGltf.Unity
{
    public interface IImporterContext : IDisposable
    {
        GltfContainer Container { get; }
        ImporterRuntimeResources RuntimeResources { get; }
        ResourcesStore BufferView { get; }

        NodeImporter Nodes { get; }
        MeshImporter Meshes { get; }
        MaterialImporter Materials { get; }
        TextureImporter Textures { get; }
        ImageImporter Images { get; }
    }
}
