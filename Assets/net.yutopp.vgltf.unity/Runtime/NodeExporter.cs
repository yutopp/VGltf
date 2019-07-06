//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VGltf.Unity
{
    public class NodeExporter : ExporterRef
    {
        public NodeExporter(Exporter parent)
            : base(parent)
        {
        }

        public IndexedResource<Transform> Export(GameObject go)
        {
            return Export(go.transform);
        }

        public IndexedResource<Transform> Export(Transform go)
        {
            return Cache.CacheObjectIfNotExists(go, Cache.Nodes, ForceExport);
        }

        public IndexedResource<Transform> ForceExport(Transform trans)
        {
            var go = trans.gameObject;

            IndexedResource<Mesh> meshResource = null;
            int? skinIndex = null;
            var mr = go.GetComponent<MeshRenderer>();
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (mr != null)
            {
                var meshFilter = mr.gameObject.GetComponent<MeshFilter>();
                var sharedMesh = meshFilter.sharedMesh;

                meshResource = Meshes.Export(mr, sharedMesh);
            }
            else if (smr != null)
            {
                var sharedMesh = smr.sharedMesh;
                meshResource = Meshes.Export(smr, sharedMesh);

                if (smr.bones.Length > 0)
                {
                    skinIndex = ExportSkin(smr, sharedMesh).Index;
                }
            }

            var t = CoordUtils.ConvertSpace(go.transform.localPosition);
            var r = CoordUtils.ConvertSpace(go.transform.localRotation);
            var s = go.transform.localScale;
            var gltfNode = new Types.Node
            {
                Name = go.name,

                Mesh = meshResource != null ? (int?)meshResource.Index : null,
                Skin = skinIndex,

                Matrix = null,
                Translation = new float[] { t.x, t.y, t.z },
                Rotation = new float[] { r.x, r.y, r.z, r.w },
                Scale = new float[] { s.x, s.y, s.z },
            };

            var nodesIndices = new List<int>();
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                var c = go.transform.GetChild(i);
                var nodeResource = Export(c.gameObject);
                nodesIndices.Add(nodeResource.Index);
            }
            if (nodesIndices.Count > 0)
            {
                gltfNode.Children = nodesIndices.ToArray();
            }

            return new IndexedResource<Transform>
            {
                Index = Types.GltfExtensions.AddNode(Gltf, gltfNode),
                Value = trans,
            };
        }

        public IndexedResource<Skin> ExportSkin(SkinnedMeshRenderer r, Mesh mesh)
        {
            return Cache.CacheObjectIfNotExists(mesh.name, mesh, Cache.Skins, (m) => ForceExportSkin(r, m));
        }

        IndexedResource<Skin> ForceExportSkin(SkinnedMeshRenderer smr, Mesh mesh)
        {
            var rootBone = smr.rootBone != null ? (int?)Export(smr.rootBone).Index : null;
            var boneIndices = smr.bones.Select(bt => Export(bt).Index).ToArray();

            var primitiveExporter = new PrimitiveExporter(this);

            int? matricesAccIndex = null;
            if (mesh.bindposes.Length > 0)
            { 
                var matrices = mesh.bindposes.Select(CoordUtils.ConvertSpace).ToArray();
                matricesAccIndex = primitiveExporter.Export(matrices);
            }

            var gltfSkin = new Types.Skin
            {
                InverseBindMatrices = matricesAccIndex,
                Skeleton = rootBone,
                Joints = boneIndices,
            };
            return new IndexedResource<Skin>{
                Index = Types.GltfExtensions.AddSkin(Gltf, gltfSkin),
                Value = new Skin(),
            };
        }
    }
}
