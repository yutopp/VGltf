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
        public IndexedResourceDict<object, Transform> Nodes = new IndexedResourceDict<object, Transform>();
        public IndexedResourceDict<string, Texture2D> Textures = new IndexedResourceDict<string, Texture2D>();
        public IndexedResourceDict<string, Material> Materials = new IndexedResourceDict<string, Material>();
        public IndexedResourceDict<string, Mesh> Meshes = new IndexedResourceDict<string, Mesh>();
        public IndexedResourceDict<string, Skin> Skins = new IndexedResourceDict<string, Skin> ();
        public IndexedResourceDict<string, Avatar> Avatars = new IndexedResourceDict<string, Avatar>();
    }
}
