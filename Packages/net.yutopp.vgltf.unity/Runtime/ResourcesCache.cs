//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using UnityEngine;

namespace VGltf.Unity
{
    public sealed class ImporterRuntimeResources : IDisposable
    {
        public IndexedResourceDict<int, GameObject> Nodes = new IndexedResourceDict<int, GameObject>();
        public IndexedResourceDict<int, Texture2D> Textures = new IndexedResourceDict<int, Texture2D>();
        public IndexedResourceDict<int, Material> Materials = new IndexedResourceDict<int, Material>();
        public IndexedResourceDict<int, Mesh> Meshes = new IndexedResourceDict<int, Mesh>();

        public void Dispose()
        {
            Nodes.Dispose();
            Meshes.Dispose();
            Materials.Dispose();
            Textures.Dispose();
        }
    }

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
