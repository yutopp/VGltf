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
            go.name = gltfNode.Name;

            var matrix = PrimitiveImporter.AsMatrix4x4(gltfNode.Matrix);
            if (!matrix.isIdentity)
            {
                throw new NotImplementedException("matrix is not implemented");
            } else
            {
                var t = gltfNode.Translation;
                var r = gltfNode.Rotation;
                var s = gltfNode.Scale;
                go.transform.localPosition = CoordUtils.ConvertSpace(PrimitiveImporter.AsVector3(t));
                go.transform.localRotation = CoordUtils.ConvertSpace(PrimitiveImporter.AsQuaternion(r));
                go.transform.localScale = PrimitiveImporter.AsVector3(s);
            }

            if (gltfNode.Mesh != null)
            {
                var meshImporter = new MeshImporter(this);
                meshImporter.Import(gltfNode.Mesh.Value, go);
            }

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
