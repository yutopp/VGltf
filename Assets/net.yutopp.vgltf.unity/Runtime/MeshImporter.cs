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
    public class MeshImporter : ImporterRef
    {
        public MeshImporter(Importer parent)
            : base(parent)
        {
        }

        public IndexedResource<Mesh> Import(int meshIndex, GameObject go)
        {
            var gltf = Container.Gltf;
            var gltfMesh = gltf.Meshes[meshIndex];

            return Cache.CacheObjectIfNotExists(meshIndex, meshIndex, Cache.Meshes, (i) => ForceImport(i, go));
        }

        class Primitive
        {
            public int Position;
            public int? Normal;
            public int? Tangent;
            public int? TexCoord0;
            public int? TexCoord1;
            public int? Color;
            public int? Weight;
            public int? Joint;

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
                        res.TexCoord0 = index;
                    }
                }

                {
                    int index;
                    if (gltfAttr.TryGetValue(Types.Mesh.PrimitiveType.AttributeName.TEXCOORD_1, out index))
                    {
                        res.TexCoord1 = index;
                    }
                }

                {
                    int index;
                    if (gltfAttr.TryGetValue(Types.Mesh.PrimitiveType.AttributeName.COLOR_0, out index))
                    {
                        res.Color = index;
                    }
                }

                {
                    int index;
                    if (gltfAttr.TryGetValue(Types.Mesh.PrimitiveType.AttributeName.WEIGHTS_0, out index))
                    {
                        res.Weight = index;
                    }
                }

                {
                    int index;
                    if (gltfAttr.TryGetValue(Types.Mesh.PrimitiveType.AttributeName.JOINTS_0, out index))
                    {
                        res.Joint = index;
                    }
                }

                if (res.Weight != null || res.Joint != null)
                {
                    if (res.Weight == null || res.Joint == null)
                    {
                        throw new NotImplementedException(""); // TODO: fix
                    }
                }

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

            var b = prims[0];
            var fullClonedMode = false;
            var skinedMesh = b.Weight != null;
            foreach (var p in prims.Skip(1))
            {
                if ((b.Position != p.Position)
                    || (b.Normal != p.Normal)
                    || (b.Tangent != p.Tangent)
                    || (b.TexCoord0 != p.TexCoord0)
                    || (b.TexCoord1 != p.TexCoord1)
                    || (b.Color != p.Color)
                    || (b.Weight != p.Weight)
                    || (b.Joint != p.Joint))
                {
                    fullClonedMode = true;
                    break;
                }

                if (skinedMesh != (p.Weight != null))
                {
                    throw new NotImplementedException(); // Renderer is mixed
                }
            }

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
                    mesh.vertices = ImportPositions(b.Position);
                }

                if (b.Normal != null)
                {
                    mesh.normals = ImportNormals(b.Normal.Value);
                }

                if (b.Tangent != null)
                {
                    mesh.tangents = ImportTangents(b.Tangent.Value);
                }

                if (b.TexCoord0 != null)
                {
                    mesh.uv = ImportUV(b.TexCoord0.Value);
                }

                if (b.TexCoord1 != null)
                {
                    mesh.uv2 = ImportUV(b.TexCoord1.Value);
                }

                if (b.Color != null)
                {
                    mesh.colors = ImportColors(b.Color.Value);
                }

                if (b.Joint != null && b.Weight != null)
                {
                    var joints = ImportJoints(b.Joint.Value);
                    var weights = ImportWeights(b.Weight.Value);
                    if (joints.Length != weights.Length)
                    {
                        throw new NotImplementedException(); // TODO
                    }

                    mesh.boneWeights = joints.Zip(weights, (j, w) => {
                        var bw = new BoneWeight();
                        bw.boneIndex0 = j.x;
                        bw.boneIndex1 = j.y;
                        bw.boneIndex2 = j.z;
                        bw.boneIndex3 = j.w;
                        bw.weight0 = w.x;
                        bw.weight1 = w.y;
                        bw.weight2 = w.z;
                        bw.weight3 = w.w;
                        return bw;
                    }).ToArray();
                }

                int submesh = 0;
                foreach (var p in prims)
                {
                    var indicesBuf = BufferView.GetOrLoadTypedBufferByAccessorIndex(p.Indices);
                    var indices = CoordUtils.FlipIndices(indicesBuf.GetPrimitivesAsCasted<int>().ToArray()).ToArray();
                    mesh.SetIndices(indices, MeshTopology.Triangles, submesh);
                    ++submesh;

                    var matResource = Materials.Import(p.Material);
                    materials.Add(matResource.Value);
                }
            }

            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            Renderer r = null;
            if (skinedMesh)
            {
                var smr = go.AddComponent<SkinnedMeshRenderer>();
                smr.sharedMesh = mesh;

                r = smr;
            } else
            {
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;

                var mr = go.AddComponent<MeshRenderer>();
                r = mr;
            }

            r.sharedMaterials = materials.ToArray();

            return new IndexedResource<Mesh>
            {
                Index = meshIndex,
                Value = mesh,
            };
        }

        // https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#meshes

        Vector3[] ImportPositions(int index)
        {
            // VEC3 | FLOAT
            var buf = BufferView.GetOrLoadTypedBufferByAccessorIndex(index);
            var acc = buf.Accessor;
            if (acc.Type == Types.Accessor.TypeEnum.Vec3)
            {
                if (acc.ComponentType == Types.Accessor.ComponentTypeEnum.FLOAT)
                {
                    return buf.GetEntity<Vector3>().GetEnumerable().Select(CoordUtils.ConvertSpace).ToArray();
                }
            }

            throw new NotImplementedException(); // TODO
        }

        Vector3[] ImportNormals(int index)
        {
            // VEC3 | FLOAT
            var buf = BufferView.GetOrLoadTypedBufferByAccessorIndex(index);
            var acc = buf.Accessor;
            if (acc.Type == Types.Accessor.TypeEnum.Vec3)
            {
                if (acc.ComponentType == Types.Accessor.ComponentTypeEnum.FLOAT)
                {
                    return buf.GetEntity<Vector3>().GetEnumerable().Select(CoordUtils.ConvertSpace).ToArray();
                }
            }

            throw new NotImplementedException(); // TODO
        }

        Vector4[] ImportTangents(int index)
        {
            // VEC4 | FLOAT
            var buf = BufferView.GetOrLoadTypedBufferByAccessorIndex(index);
            var acc = buf.Accessor;
            if (acc.Type == Types.Accessor.TypeEnum.Vec4)
            {
                if (acc.ComponentType == Types.Accessor.ComponentTypeEnum.FLOAT)
                {
                    return buf.GetEntity<Vector4>().GetEnumerable().Select(CoordUtils.ConvertSpace).ToArray();
                }
            }

            throw new NotImplementedException(); // TODO
        }

        Vector2[] ImportUV(int index)
        {
            // VEC2 | FLOAT
            //      | UNSIGNED_BYTE  (normalized) 
            //      | UNSIGNED_SHORT (normalized)
            var buf = BufferView.GetOrLoadTypedBufferByAccessorIndex(index);
            var acc = buf.Accessor;
            if (acc.Type == Types.Accessor.TypeEnum.Vec2)
            {
                if (acc.ComponentType == Types.Accessor.ComponentTypeEnum.FLOAT)
                {
                    return buf.GetEntity<Vector2>().GetEnumerable().Select(CoordUtils.ConvertUV).ToArray();
                }
            }

            throw new NotImplementedException(); // TODO
        }

        Color[] ImportColors(int index)
        {
            // VEC3 | FLOAT
            // VEC4 | UNSIGNED_BYTE  (normalized)
            //      | UNSIGNED_SHORT (normalized)
            var buf = BufferView.GetOrLoadTypedBufferByAccessorIndex(index);
            var acc = buf.Accessor;
            if (acc.Type == Types.Accessor.TypeEnum.Vec4)
            {
                if (acc.ComponentType == Types.Accessor.ComponentTypeEnum.FLOAT)
                {
                    return buf.GetEntity<Color>().GetEnumerable().ToArray();
                }
            }

            throw new NotImplementedException(); // TODO
        }

        Vec4<int>[] ImportJoints(int index)
        {
            // VEC4 | UNSIGNED_BYTE
            //      | UNSIGNED_SHORT
            var buf = BufferView.GetOrLoadTypedBufferByAccessorIndex(index);
            var acc = buf.Accessor;
            if (acc.Type == Types.Accessor.TypeEnum.Vec4)
            {
                if (acc.ComponentType == Types.Accessor.ComponentTypeEnum.UNSIGNED_SHORT)
                {
                    return buf.GetEntity<Vec4<ushort>>()
                        .GetEnumerable()
                        .Select(v => new Vec4<int>(v.x, v.y, v.z, v.w))
                        .ToArray();
                }
            }

            throw new NotImplementedException(); // TODO
        }

        Vector4[] ImportWeights(int index)
        {
            // VEC4 | FLOAT
            //      | UNSIGNED_BYTE  (normalized)
            //      | UNSIGNED_SHORT (normalized)
            var buf = BufferView.GetOrLoadTypedBufferByAccessorIndex(index);
            var acc = buf.Accessor;
            if (acc.Type == Types.Accessor.TypeEnum.Vec4)
            {
                if (acc.ComponentType == Types.Accessor.ComponentTypeEnum.FLOAT)
                {
                    return buf.GetEntity<Vector4>().GetEnumerable().ToArray();
                }
            }

            throw new NotImplementedException(); // TODO
        }
    }
}
