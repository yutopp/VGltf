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
    public class PrimitiveExporter : ExporterRef
    {
        public PrimitiveExporter(ExporterRef parent)
            : base(parent)
        {
        }

        public static float[] AsArray(Vector3 v)
        {
            return new float[] { v.x, v.y, v.z };
        }

        public static float[] AsArray(Vector4 v)
        {
            return new float[] { v.x, v.y, v.z, v.w };
        }

        public static float[] AsArray(Color c)
        {
            return new float[] { c.r, c.g, c.b, c.a };
        }

        public static float[] AsArray(Quaternion q)
        {
            return new float[] { q.x, q.y, q.z, q.w };
        }

        public static float[] AsArray(Matrix4x4 m)
        {
            // column-major
            return new float[]
            {
                m.m00, m.m10, m.m20, m.m30,
                m.m01, m.m11, m.m21, m.m31,
                m.m02, m.m12, m.m22, m.m32,
                m.m03, m.m13, m.m23, m.m33,
            };
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




        public int Export(Matrix4x4[] mat4x4)
        {
            byte[] buffer = Marshal(mat4x4);

            var viewIndex = BufferBuilder.AddView(new ArraySegment<byte>(buffer));//, null, Types.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER);

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.FLOAT,
                Count = mat4x4.Length,
                Type = Types.Accessor.TypeEnum.Mat4,
            };
            return Types.GltfExtensions.AddAccessor(Gltf, accessor);
        }



        public static byte[] Marshal(int[] arr)
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

        public static byte[] Marshal(Vector2[] vec2)
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

        public static byte[] Marshal(Vector3[] vec3)
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

        public static byte[] Marshal(Vector4[] vec4)
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

        public static byte[] Marshal(Vector4 vec4)
        {
            // TODO: optimize
            byte[] buffer;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(vec4.x);
                w.Write(vec4.y);
                w.Write(vec4.z);
                w.Write(vec4.w);

                w.Flush();
                buffer = ms.ToArray();
            }
            return buffer;
        }

        public static byte[] Marshal(Vec4<ushort>[] vec4)
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

        public static byte[] Marshal(Color[] c)
        {
            // TODO: optimize
            byte[] buffer;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                foreach (var v in c)
                {
                    w.Write(v.r);
                    w.Write(v.g);
                    w.Write(v.b);
                    w.Write(v.a);
                }
                w.Flush();
                buffer = ms.ToArray();
            }
            return buffer;
        }

        static byte[] Marshal(Matrix4x4[] mat4x4)
        {
            // TODO: optimize
            byte[] buffer;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                foreach (var m in mat4x4)
                {
                    w.Write(Marshal(m.GetColumn(0)));
                    w.Write(Marshal(m.GetColumn(1)));
                    w.Write(Marshal(m.GetColumn(2)));
                    w.Write(Marshal(m.GetColumn(3)));
                }
                w.Flush();
                buffer = ms.ToArray();
            }
            return buffer;
        }
    }
}
