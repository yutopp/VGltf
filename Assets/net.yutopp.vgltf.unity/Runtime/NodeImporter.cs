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
    public class NodeImporter : ImporterBase
    {
        public NodeImporter(ImporterBase parent)
            : base(parent)
        {
        }
        public GameObject Import(int nodeIndex)
        {
            var gltf = Container.Gltf;
            var gltfNode = gltf.Nodes[nodeIndex];

            var go = new GameObject();

            if (gltfNode.Children != null)
            {
                foreach (var child in gltfNode.Children)
                {
                    var childGo = Import(child);
                    childGo.transform.SetParent(go.transform);
                }
            }

            return go;
        }
    }
}
