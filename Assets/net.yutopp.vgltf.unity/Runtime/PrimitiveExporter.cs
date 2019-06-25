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
    class PrimitiveExporter : ExporterBase
    {
        public PrimitiveExporter(ExporterBase parent)
            : base(parent)
        {
        }

        public int Export(int[] arr)
        {
            // TODO: Fix(optimize) size (ComponentType)
            byte[] buffer = Marshal(arr);

            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));//, null, Types.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER);

            var min = int.MaxValue;
            var max = int.MinValue;
            foreach (var v in arr)
            {
                min = Mathf.Min(v, min);
                max = Mathf.Max(v, max);
            }

            // TODO: Fix(optimize) size (ComponentType)
            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.UNSIGNED_INT,
                Count = arr.Length,
                Type = Types.Accessor.TypeEnum.Scalar,
                Min = new float[] { min },
                Max = new float[] { max },
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }

        public int Export(Vector2[] vec2)
        {
            byte[] buffer = Marshal(vec2);

            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));//, null, Types.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER);

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var v in vec2)
            {
                min = new Vector2(Mathf.Min(v.x, min.x), Mathf.Min(v.y, min.y));
                max = new Vector2(Mathf.Max(v.x, max.x), Mathf.Max(v.y, max.y));
            }

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.FLOAT,
                Count = vec2.Length,
                Type = Types.Accessor.TypeEnum.Vec2,
                Min = new float[] { min.x, min.y },
                Max = new float[] { max.x, max.y },
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }

        public int Export(Vector3[] vec3)
        {
            byte[] buffer = Marshal(vec3);

            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));//, null, Types.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER);

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

        public int Export(Vector4[] vec4)
        {
            byte[] buffer = Marshal(vec4);

            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));//, null, Types.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER);

            var min = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
            foreach (var v in vec4)
            {
                min = new Vector4(Mathf.Min(v.x, min.x), Mathf.Min(v.y, min.y), Mathf.Min(v.z, min.z), Mathf.Min(v.w, min.w));
                max = new Vector4(Mathf.Max(v.x, max.x), Mathf.Max(v.y, max.y), Mathf.Max(v.z, max.z), Mathf.Max(v.w, max.w));
            }

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.FLOAT,
                Count = vec4.Length,
                Type = Types.Accessor.TypeEnum.Vec4,
                Min = new float[] { min.x, min.y, min.z, min.w },
                Max = new float[] { max.x, max.y, max.z, max.w },
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }

        static byte[] Marshal(int[] arr)
        {
            // TODO: optimize
            byte[] buffer;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                foreach (var v in arr)
                {
                    w.Write(v);
                }
                w.Flush();
                buffer = ms.ToArray();
            }
            return buffer;
        }

        static byte[] Marshal(Vector2[] vec2)
        {
            // TODO: optimize
            byte[] buffer;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                foreach (var v in vec2)
                {
                    w.Write(v.x);
                    w.Write(v.y);
                }
                w.Flush();
                buffer = ms.ToArray();
            }
            return buffer;
        }

        static byte[] Marshal(Vector3[] vec3)
        {
            // TODO: optimize
            byte[] buffer;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                foreach (var v in vec3)
                {
                    w.Write(v.x);
                    w.Write(v.y);
                    w.Write(v.z);
                }
                w.Flush();
                buffer = ms.ToArray();
            }
            return buffer;
        }

        static byte[] Marshal(Vector4[] vec4)
        {
            // TODO: optimize
            byte[] buffer;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                foreach (var v in vec4)
                {
                    w.Write(v.x);
                    w.Write(v.y);
                    w.Write(v.z);
                    w.Write(v.w);
                }
                w.Flush();
                buffer = ms.ToArray();
            }
            return buffer;
        }
    }
}
