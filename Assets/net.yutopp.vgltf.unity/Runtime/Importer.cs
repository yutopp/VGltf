//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VGltf.Unity
{
    public class Importer : IImporter, IDisposable
    {
        public GltfContainer Container { get; }
        public ResourcesCache<int> Cache { get; }
        public ResourcesStore BufferView { get; }

        public NodeImporter Nodes { get; }
        public MeshImporter Meshes { get; }
        public MaterialImporter Materials { get; }
        public TextureImporter Textures { get; }
        public ImageImporter Images { get; }

        public Importer(GltfContainer container, IResourceLoader loader)
        {
            Container = container;
            Cache = new ResourcesCache<int>();
            BufferView = new ResourcesStore(container.Gltf, container.Buffer, loader);

            Nodes = new NodeImporter(this);
            Meshes = new MeshImporter(this);
            Materials = new MaterialImporter(this);
            Textures = new TextureImporter(this);
            Images = new ImageImporter(this);
        }

        public Importer(GltfContainer container)
            : this(container, new ResourceLoaderFromEmbedOnly())
        {
        }

        public GameObject ImportSceneAsGameObject()
        {
            var gltf = Container.Gltf;
            if (gltf.Scene == null)
            {
                return null;
            }

            var gltfScene = gltf.Scenes[gltf.Scene.Value];
            if (gltfScene.Nodes.Length != 1)
            {
                // TODO: raise an exception
                return null;
            }

            var nodeIndex = gltfScene.Nodes[0];
            var go = Nodes.Import(nodeIndex);

            return go;
        }

        public void Dispose()
        {
            // TODO: Remove resources
        }
    }
}
