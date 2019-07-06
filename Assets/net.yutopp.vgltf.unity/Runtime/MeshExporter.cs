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
    public class MeshExporter : ExporterRef
    {
        public MeshExporter(Exporter parent)
            : base(parent)
        {
        }

        public IndexedResource<Mesh> Export(Renderer r, Mesh mesh)
        {
            return Cache.CacheObjectIfNotExists(mesh.name, mesh, Cache.Meshes, (m) => ForceExport(r, m));
        }

        public IndexedResource<Mesh> ForceExport(Renderer r, Mesh mesh)
        {
            var materialIndices = new List<int>();
            foreach (var m in r.sharedMaterials)
            {
                var materialResource = Materials.Export(m);
                materialIndices.Add(materialResource.Index);
            }

            var primitiveExporter = new PrimitiveExporter(this);

            // Convert to right-handed coordinate system
            var positionAccIndex = ExportPositions(mesh.vertices);

            int? normalAccIndex = null;
            if (mesh.normals.Length > 0)
            {
                normalAccIndex = ExportNormals(mesh.normals);
            }

            int? tangentAccIndex = null;
            //if (mesh.tangents.Length > 0)
            //{
            //    tangentAccIndex = ExportTangents(mesh.tangents);
            //}

            int? texcoord0AccIndex = null;
            if (mesh.uv.Length > 0)
            {
                texcoord0AccIndex = ExportUV(mesh.uv);
            }

            int? texcoord1AccIndex = null;
            if (mesh.uv2.Length > 0)
            {
                texcoord1AccIndex = ExportUV(mesh.uv2);
            }

            int? color0AccIndex = null;
            if (mesh.colors.Length > 0)
            {
                color0AccIndex = ExportColors(mesh.colors);
            }

            int? joints0AccIndex = null;
            int? weights0AccIndex = null;
            if (mesh.boneWeights.Length > 0)
            {
                var joints = mesh.boneWeights
                    .Select(w => new Vec4<int>(
                        w.boneIndex0,
                        w.boneIndex1,
                        w.boneIndex2,
                        w.boneIndex3)
                    ).ToArray();
                var weights = mesh.boneWeights
                    .Select(w => new Vector4(
                        w.weight0,
                        w.weight1,
                        w.weight2,
                        w.weight3)
                    ).ToArray();

                joints0AccIndex = ExportJoints(joints);
                weights0AccIndex = ExportWeights(weights);
            }

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
                if (tangentAccIndex != null)
                {
                    attrs[Types.Mesh.PrimitiveType.AttributeName.TANGENT] = tangentAccIndex.Value;
                }
                if (texcoord0AccIndex != null)
                {
                    attrs[Types.Mesh.PrimitiveType.AttributeName.TEXCOORD_0] = texcoord0AccIndex.Value;
                }
                if (texcoord1AccIndex != null)
                {
                    attrs[Types.Mesh.PrimitiveType.AttributeName.TEXCOORD_1] = texcoord1AccIndex.Value;
                }
                if (color0AccIndex != null)
                {
                    attrs[Types.Mesh.PrimitiveType.AttributeName.COLOR_0] = color0AccIndex.Value;
                }
                if (joints0AccIndex != null)
                {
                    attrs[Types.Mesh.PrimitiveType.AttributeName.JOINTS_0] = joints0AccIndex.Value;
                }
                if (weights0AccIndex != null)
                {
                    attrs[Types.Mesh.PrimitiveType.AttributeName.WEIGHTS_0] = weights0AccIndex.Value;
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
                Name = mesh.name,

                Primitives = primitives,
                // Weights = TODO: Support morph targets
            };
            return new IndexedResource<Mesh>
            {
                Index = Types.GltfExtensions.AddMesh(Gltf, gltfMesh),
                Value = mesh,
            };
        }

        // https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#meshes

        int ExportPositions(Vector3[] vec3)
        {
            vec3 = vec3.Select(CoordUtils.ConvertSpace).ToArray();

            // VEC3! | FLOAT!
            byte[] buffer = PrimitiveExporter.Marshal(vec3);
            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));

            // position MUST have min/max
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var v in vec3)
            {
                min = new Vector3(Mathf.Min(v.x, min.x), Mathf.Min(v.y, min.y), Mathf.Min(v.z, min.z));
                max = new Vector3(Mathf.Max(v.x, max.x), Mathf.Max(v.y, max.y), Mathf.Max(v.z, max.z));
            }

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.FLOAT,
                Count = vec3.Length,
                Type = Types.Accessor.TypeEnum.Vec3,
                Min = new float[] { min.x, min.y, min.z },
                Max = new float[] { max.x, max.y, max.z },
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }

        int ExportNormals(Vector3[] vec3)
        {
            vec3 = vec3.Select(CoordUtils.ConvertSpace).ToArray();

            // VEC3! | FLOAT!
            byte[] buffer = PrimitiveExporter.Marshal(vec3);
            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.FLOAT,
                Count = vec3.Length,
                Type = Types.Accessor.TypeEnum.Vec3,
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }

        int ExportTangents(Vector4[] vec4)
        {
            vec4 = vec4.Select(CoordUtils.ConvertSpace).ToArray();

            // VEC4! | FLOAT!
            byte[] buffer = PrimitiveExporter.Marshal(vec4);
            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.FLOAT,
                Count = vec4.Length,
                Type = Types.Accessor.TypeEnum.Vec4,
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }

        int ExportUV(Vector2[] uv)
        {
            uv = uv.Select(CoordUtils.ConvertUV).ToArray();

            // VEC2! | FLOAT!
            //       | UNSIGNED_BYTE  (normalized) 
            //       | UNSIGNED_SHORT (normalized)
            byte[] buffer = PrimitiveExporter.Marshal(uv);
            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.FLOAT,
                Count = uv.Length,
                Type = Types.Accessor.TypeEnum.Vec2,
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }

        int ExportColors(Color[] colors)
        {
            // VEC3  | FLOAT!
            // VEC4! | UNSIGNED_BYTE  (normalized)
            //       | UNSIGNED_SHORT (normalized)
            byte[] buffer = PrimitiveExporter.Marshal(colors);
            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.FLOAT,
                Count = colors.Length,
                Type = Types.Accessor.TypeEnum.Vec4,
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }

        int ExportJoints(Vec4<int>[] joints)
        {
            // VEC4! | UNSIGNED_BYTE
            //       | UNSIGNED_SHORT!
            byte[] buffer = PrimitiveExporter.Marshal(
                joints
                .Select(v => new Vec4<ushort>((ushort)v.x, (ushort)v.y, (ushort)v.z, (ushort)v.w))
                .ToArray()
                );
            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                Count = joints.Length,
                Type = Types.Accessor.TypeEnum.Vec4,
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }

        int ExportWeights(Vector4[] weights)
        {
            // VEC4! | FLOAT!
            //       | UNSIGNED_BYTE  (normalized)
            //       | UNSIGNED_SHORT (normalized)
            byte[] buffer = PrimitiveExporter.Marshal(weights);
            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.FLOAT,
                Count = weights.Length,
                Type = Types.Accessor.TypeEnum.Vec4,
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }
    }
}
