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
    public sealed class ExporterRuntimeResources : IDisposable
    {
        public IndexedResourceDict<string, GameObject> Nodes = new IndexedResourceDict<string, GameObject>();
        public IndexedResourceDict<string, Texture> Textures = new IndexedResourceDict<string, Texture>();
        public IndexedResourceDict<string, Material> Materials = new IndexedResourceDict<string, Material>();
        public IndexedResourceDict<string, Mesh> Meshes = new IndexedResourceDict<string, Mesh>();
        public IndexedResourceDict<string, Skin> Skins = new IndexedResourceDict<string, Skin>();

        public void Dispose()
        {
            // DO NOT Dispose any resources because these containers have no ownerships.
        }
    }
}
