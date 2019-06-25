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
    public class NodeExporter : ExporterBase
    {
        public NodeExporter(ExporterBase parent)
            : base(parent)
        {
        }

        public int? Export(GameObject go)
        {
            var meshExporter = new MeshExporter(this);

            IndexedResource<Mesh> meshResource = null;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                meshResource = meshExporter.Export(mr);
            }
            else
            {
                var smr = go.GetComponent<SkinnedMeshRenderer>();
                if (smr != null)
                {
                    meshResource = meshExporter.Export(smr);
                }
            }

            var t = CoordUtils.ConvertSpace(go.transform.localPosition);
            var r = CoordUtils.ConvertSpace(go.transform.localRotation);
            var s = go.transform.localScale;
            var gltfNode = new Types.Node
            {
                Mesh = meshResource != null ? (int?)meshResource.Index : null,
                Matrix = null,
                Translation = new float[] { t.x, t.y, t.z },
                Rotation = new float[] { r.x, r.y, r.z, r.w },
                Scale = new float[] { s.x, s.y, s.z },
            };

            var nodesIndices = new List<int>();
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                var c = go.transform.GetChild(i);
                var nodeIndex = Export(c.gameObject);
                if (nodeIndex != null)
                {
                    nodesIndices.Add(nodeIndex.Value);
                }
            }
            if (nodesIndices.Count > 0)
            {
                gltfNode.Children = nodesIndices.ToArray();
            }

            if (gltfNode.Mesh == null && gltfNode.Children == null)
            {
                return null; // This is an empty node. Do not add.
            }

            return Types.GltfExtensions.AddNode(Gltf, gltfNode);
        }
    }
}
