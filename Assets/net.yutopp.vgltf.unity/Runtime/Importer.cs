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
    public class Importer : ImporterBase, IDisposable
    {
        public Importer(GltfContainer container)
            : base(container, new ResourcesCache())
        {
        }

        public GameObject Import()
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

            var nodeImporter = new NodeImporter(this);
            var go = nodeImporter.Import(nodeIndex);

            return go;
        }

        public void Dispose()
        {
            // TODO: Remove resources
        }
    }
}
