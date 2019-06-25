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
    public class MeshExporter : ExporterBase
    {
        public MeshExporter(ExporterBase parent)
            : base(parent)
        {
        }

        public IndexedResource<Mesh> Export(MeshRenderer r)
        {
            var meshFilter = r.gameObject.GetComponent<MeshFilter>();
            var sharedMesh = meshFilter.sharedMesh;

            return Cache.CacheObjectIfNotExists(sharedMesh.name, sharedMesh, Cache.Meshes, (m) => ForceExport(r, m));
        }

        public IndexedResource<Mesh> Export(SkinnedMeshRenderer r)
        {
            var sharedMesh = r.sharedMesh;

            return Cache.CacheObjectIfNotExists(sharedMesh.name, sharedMesh, Cache.Meshes, (m) => ForceExport(r, m));
        }

        public IndexedResource<Mesh> ForceExport(Renderer r, Mesh mesh)
        {
            var materialExporter = new MaterialExporter(this);
            var materialIndices = new List<int>();
            foreach (var m in r.sharedMaterials)
            {
                var materialResource = materialExporter.Export(m);
                materialIndices.Add(materialResource.Index);
            }

            var primitiveExporter = new PrimitiveExporter(this);

            // Convert to right-handed coordinate system
            var positionAccIndex = primitiveExporter.Export(mesh.vertices.Select(CoordUtils.ConvertSpace).ToArray());

            int? normalAccIndex = null;
            if (mesh.normals.Length > 0)
            {
                normalAccIndex = primitiveExporter.Export(mesh.normals.Select(CoordUtils.ConvertSpace).ToArray());
            }

            int? tantentAccIndex = null;
            if (mesh.tangents.Length > 0)
            {
                tantentAccIndex = primitiveExporter.Export(mesh.tangents.Select(CoordUtils.ConvertSpace).ToArray());
            }

            int? texcoord0AccIndex = null;
            if (mesh.uv.Length > 0)
            {
                texcoord0AccIndex = primitiveExporter.Export(mesh.uv.Select(CoordUtils.ConvertUV).ToArray());
            }

            int? texcoord1AccIndex = null;
            if (mesh.uv2.Length > 0)
            {
                texcoord1AccIndex = primitiveExporter.Export(mesh.uv.Select(CoordUtils.ConvertUV).ToArray());
            }
            // TODO: COLOR_0, JOINTS_0, WEIGHTS_0

            var primitives = new List<Types.Mesh.PrimitiveType>();
            for (var i = 0; i < mesh.subMeshCount; ++i)
            {
                var indices = mesh.GetIndices(i);
                var positionindicesAccIndex = primitiveExporter.Export(CoordUtils.FlipIndices(indices).ToArray());

                var attrs = new Dictionary<string, int>();
                attrs[Types.Mesh.PrimitiveType.AttributeName.POSITION] = positionAccIndex;
                if (normalAccIndex != null)
                {
                    attrs[Types.Mesh.PrimitiveType.AttributeName.NORMAL] = normalAccIndex.Value;
                }
                if (tantentAccIndex != null)
                {
                    attrs[Types.Mesh.PrimitiveType.AttributeName.TANGENT] = tantentAccIndex.Value;
                }
                if (texcoord0AccIndex != null)
                {
                    attrs[Types.Mesh.PrimitiveType.AttributeName.TEXCOORD_0] = texcoord0AccIndex.Value;
                }
                if (texcoord1AccIndex != null)
                {
                    attrs[Types.Mesh.PrimitiveType.AttributeName.TEXCOORD_1] = texcoord1AccIndex.Value;
                }

                var primitive = new Types.Mesh.PrimitiveType
                {
                    Attributes = attrs,
                    Indices = positionindicesAccIndex,
                    Material = materialIndices[i < materialIndices.Count ? i : materialIndices.Count - 1],
                    // Targets = TODO: Support morph targets
                };
                primitives.Add(primitive);
            }

            var gltfMesh = new Types.Mesh
            {
                Primitives = primitives,
                // Weights = TODO: Support morph targets
            };
            return new IndexedResource<Mesh>
            {
                Index = Types.GltfExtensions.AddMesh(Gltf, gltfMesh),
                Value = mesh,
            };
        }
    }
}
