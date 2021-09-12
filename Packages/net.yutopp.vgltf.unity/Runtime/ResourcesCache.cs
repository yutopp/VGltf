//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using UnityEngine;

namespace VGltf.Unity
{
    public class ImporterRuntimeResources
    {
        public IndexedResourceDict<int, Transform> Nodes = new IndexedResourceDict<int, Transform>();
        public IndexedResourceDict<int, Texture2D> Textures = new IndexedResourceDict<int, Texture2D>();
        public IndexedResourceDict<int, Material> Materials = new IndexedResourceDict<int, Material>();
        public IndexedResourceDict<int, Mesh> Meshes = new IndexedResourceDict<int, Mesh>();
        public IndexedResourceDict<int, Skin> Skins = new IndexedResourceDict<int, Skin>();
        public IndexedResourceDict<int, Avatar> Avatars = new IndexedResourceDict<int, Avatar>();
    }

    public class ExporterRuntimeResources
    {
        public IndexedResourceDict<Transform, Transform> Nodes = new IndexedResourceDict<Transform, Transform>();
        public IndexedResourceDict<Texture, Texture2D> Textures = new IndexedResourceDict<Texture, Texture2D>();
        public IndexedResourceDict<Material, Material> Materials = new IndexedResourceDict<Material, Material>();
        public IndexedResourceDict<Mesh, Mesh> Meshes = new IndexedResourceDict<Mesh, Mesh>();
        public IndexedResourceDict<Mesh, Skin> Skins = new IndexedResourceDict<Mesh, Skin> ();
    }
}
