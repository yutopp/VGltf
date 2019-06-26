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
    class MeshImporter : ImporterBase
    {
        public MeshImporter(ImporterBase parent)
            : base(parent)
        {
        }

        public IndexedResource<Mesh> Import(int meshIndex, GameObject go)
        {
            var gltf = Container.Gltf;
            var gltfMesh = gltf.Meshes[meshIndex];

            return Cache.CacheObjectIfNotExists(gltfMesh.Name, meshIndex, Cache.Meshes, (i) => ForceImport(i, go));
        }

        class Primitive
        {
            public int Position;
            public int? Normal;
            public int? Tangent;
            public int? UV;
            public int? UV2;
            public int? Color;

            public int Indices;
            public int Material;
        }

        public IndexedResource<Mesh> ForceImport(int meshIndex, GameObject go)
        {
            var gltf = Container.Gltf;
            var gltfMesh = gltf.Meshes[meshIndex];

            var mesh = new Mesh();
            mesh.name = gltfMesh.Name;
            mesh.subMeshCount = gltfMesh.Primitives.Count;

            var prims = gltfMesh.Primitives.Select(p => {
                var gltfAttr = p.Attributes;
                var res = new Primitive();

                {
                    int index;
                    if (!gltfAttr.TryGetValue(Types.Mesh.PrimitiveType.AttributeName.POSITION, out index))
                    {
                        throw new NotImplementedException(""); // TODO: fix
                    }
                    res.Position = index;
                }

                {
                    int index;
                    if (gltfAttr.TryGetValue(Types.Mesh.PrimitiveType.AttributeName.NORMAL, out index))
                    {
                        res.Normal = index;
                    }
                }

                {
                    int index;
                    if (gltfAttr.TryGetValue(Types.Mesh.PrimitiveType.AttributeName.TANGENT, out index))
                    {
                        res.Tangent = index;
                    }
                }

                {
                    int index;
                    if (gltfAttr.TryGetValue(Types.Mesh.PrimitiveType.AttributeName.TEXCOORD_0, out index))
                    {
                        res.UV = index;
                    }
                }

                {
                    int index;
                    if (gltfAttr.TryGetValue(Types.Mesh.PrimitiveType.AttributeName.TEXCOORD_1, out index))
                    {
                        res.UV2 = index;
                    }
                }

                {
                    int index;
                    if (gltfAttr.TryGetValue(Types.Mesh.PrimitiveType.AttributeName.COLOR_0, out index))
                    {
                        res.Color = index;
                    }
                }
                // TODO: COLOR_0, JOINTS_0, WEIGHTS_0

                if (p.Indices == null)
                {
                    throw new NotImplementedException(""); // TODO: fix
                }
                res.Indices = p.Indices.Value;

                if (p.Material == null)
                {
                    throw new NotImplementedException(""); // TODO: fix
                }
                res.Material = p.Material.Value;

                return res;
            }).ToArray();

            var fullClonedMode = false;
            var b = prims[0];
            foreach (var p in prims.Skip(1))
            {
                if ((b.Position != p.Position)
                    || (b.Normal != p.Normal))
                {
                    fullClonedMode = true;
                    break;
                }
            }

            var materialImporter = new MaterialImporter(this);
            var materials = new List<Material>();

            if (fullClonedMode)
            {
                throw new NotImplementedException("Not supported");

                IEnumerable<Vector3> vertices = new Vector3[] { };
                IEnumerable<Vector3> normals = new Vector3[] { };

                var currentOffset = 0;
                foreach (var p in prims)
                {
                    var positionBuf = BufferView.GetOrLoadTypedBufferByAccessorIndex(p.Position);
                    var pVertices = positionBuf.GetEntity<Vector3>().GetEnumerable().Select(CoordUtils.ConvertSpace).ToArray();
                }
            } else
            {
                {
                    var view = BufferView.GetOrLoadTypedBufferByAccessorIndex(b.Position);
                    var buffer = view.GetEntity<Vector3>().GetEnumerable().Select(CoordUtils.ConvertSpace).ToArray();
                    mesh.vertices = buffer;
                }

                if (b.Normal != null)
                {
                    var view = BufferView.GetOrLoadTypedBufferByAccessorIndex(b.Normal.Value);
                    var buffer = view.GetEntity<Vector3>().GetEnumerable().Select(CoordUtils.ConvertSpace).ToArray();
                    mesh.normals = buffer;
                }

                if (b.Tangent != null)
                {
                    var view = BufferView.GetOrLoadTypedBufferByAccessorIndex(b.Tangent.Value);
                    var buffer = view.GetEntity<Vector4>().GetEnumerable().Select(CoordUtils.ConvertSpace).ToArray();
                    mesh.tangents = buffer;
                }

                if (b.UV != null)
                {
                    var view = BufferView.GetOrLoadTypedBufferByAccessorIndex(b.UV.Value);
                    var buffer = view.GetEntity<Vector2>().GetEnumerable().Select(CoordUtils.ConvertUV).ToArray();
                    mesh.uv = buffer;
                }

                if (b.UV2 != null)
                {
                    var view = BufferView.GetOrLoadTypedBufferByAccessorIndex(b.UV2.Value);
                    var buffer = view.GetEntity<Vector2>().GetEnumerable().Select(CoordUtils.ConvertUV).ToArray();
                    mesh.uv2 = buffer;
                }

                int submesh = 0;
                foreach (var p in prims)
                {
                    var indicesBuf = BufferView.GetOrLoadTypedBufferByAccessorIndex(p.Indices);
                    var indices = CoordUtils.FlipIndices(indicesBuf.GetPrimitivesAsCasted<int>().ToArray()).ToArray();
                    mesh.SetIndices(indices, MeshTopology.Triangles, submesh);
                    ++submesh;

                    var matResource = materialImporter.Import(p.Material);
                    materials.Add(matResource.Value);
                }
            }

            //var mf = go.AddComponent<MeshFilter>();
            //mf.sharedMesh = mesh;

            var mr = go.AddComponent<SkinnedMeshRenderer>();
            mr.sharedMesh = mesh;
            mr.sharedMaterials = materials.ToArray();

            return new IndexedResource<Mesh>
            {
                Index = meshIndex,
                Value = mesh,
            };
        }
    }
}
