//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VGltf.Types;

namespace VGltf
{
    public interface ITypedView<T> where T : struct
    {
        IEnumerable<T> GetEnumerable();
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class TypedArray<T> : ITypedView<T> where T : struct
    {
        public T[] TypedBuffer { get; }

        public TypedArray(T[] typedBuffer)
        {
            TypedBuffer = typedBuffer;
        }

        // TODO: fix performance
        public IEnumerable<T> GetEnumerable()
        {
            return TypedBuffer;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class TypedArrayView<T> : ITypedView<T> where T : struct
    {
        public ArraySegment<T> TypedBuffer { get; }

        public TypedArrayView(ArraySegment<T> typedBuffer)
        {
            TypedBuffer = typedBuffer;
        }

        // TODO: fix performance
        public IEnumerable<T> GetEnumerable()
        {
            // To support .NET3.5...
            for (var i = TypedBuffer.Offset; i < TypedBuffer.Offset + TypedBuffer.Count; ++i)
            {
                yield return TypedBuffer.Array[i];
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class TypedArrayStorage<T> : ITypedView<T> where T : struct
    {
        public ArraySegment<byte> RawBuffer { get; }
        public T[] TypedBuffer { get; }

        public TypedArrayStorage(ArraySegment<byte> rawBuffer,
                                 int byteOffset,
                                 int elemSize,
                                 int elemNum)
        {
            RawBuffer = rawBuffer;

            // All buffer data are little endian.
            // See: https://github.com/KhronosGroup/glTF/blob/8e3810c01a04930a8c98b2d76232b63f4dab944f/specification/2.0/Specification.adoc#36-binary-data-storage
            //
            // TODO: If you use this library on machines which have other endianness, need to implement supporting that.
            //

            // Deep copy...
            TypedBuffer = new T[elemNum];
            GCHandle gch = GCHandle.Alloc(TypedBuffer, GCHandleType.Pinned);
            try
            {
                //System.Buffer.BlockCopy(RawBuffer.Array, RawBuffer.Offset, TypedBuffer, 0, elemSize * elemNum);
                Marshal.Copy(RawBuffer.Array, RawBuffer.Offset, gch.AddrOfPinnedObject(), elemSize * elemNum);
            }
            finally
            {
                gch.Free();
            }
        }

        public IEnumerable<T> GetEnumerable()
        {
            return TypedBuffer;
        }

        public ITypedView<U> CastTo<U>() where U : struct
        {
            return new TypedArray<U>(TypedBuffer.Select(x => (U)Convert.ChangeType(x, typeof(U))).ToArray());
        }
    }

    public sealed class TypedArrayStorageFromBufferView<T> : ITypedView<T> where T : struct
    {
        public TypedArrayStorage<T> Storage { get; }

        public TypedArrayStorageFromBufferView(ResourcesStore store,
                                               int bufferViewIndex,
                                               int byteOffset,
                                               int componentSize, // Size of primitives (e.g. int = 4)
                                               int componentNum,  // Number of primitives in compound values (e.g. VEC3 = 3)
                                               int count)         // Number of compound values (e.g. Number of VEC3)
        {
            var bufferView = store.Gltf.BufferViews[bufferViewIndex];
            var r = store.GetOrLoadBufferViewResourceAt(bufferViewIndex);

            var compoundSize = componentSize * componentNum; // Size in bytes sizeof(int * VEC3) = 4 * 3
            var stride = bufferView.ByteStride != null ? bufferView.ByteStride.Value : compoundSize;

            var buffer = new ArraySegment<byte>(r.Data.Array, r.Data.Offset + byteOffset, count * stride);

            Storage = new TypedArrayStorage<T>(buffer, byteOffset, stride, count);
        }

        public IEnumerable<T> GetEnumerable()
        {
            return Storage.GetEnumerable();
        }

        public ITypedView<U> CastTo<U>() where U : struct
        {
            return Storage.CastTo<U>();
        }
    }

    public sealed class TypedArrayEntity<T> : ITypedView<T> where T : struct
    {
        public ITypedView<T> DenseView { get; }

        public ITypedView<uint> SparseIndices { get; }
        public ITypedView<T> SparseValues { get; }

        public int Length { get; }

        public TypedArrayEntity(ResourcesStore store, Accessor accessor)
        {
            Length = accessor.Count;

            if (accessor.BufferView != null)
            {
                DenseView = new TypedArrayStorageFromBufferView<T>(
                    store, accessor.BufferView.Value,
                    accessor.ByteOffset,
                    accessor.ComponentType.SizeInBytes(),
                    accessor.Type.NumOfComponents(),
                    accessor.Count);
            }

            if (accessor.Sparse != null)
            {
                var sparse = accessor.Sparse;

                var indices = sparse.Indices;
                switch (indices.ComponentType)
                {
                    case Types.Accessor.SparseType.IndicesType.ComponentTypeEnum.UNSIGNED_BYTE:
                        SparseIndices = new TypedArrayStorageFromBufferView<byte>(
                            store, indices.BufferView,
                            indices.ByteOffset,
                            indices.ComponentType.SizeInBytes(),
                            1, // must be scalar
                            sparse.Count
                            ).CastTo<uint>();
                        break;

                    case Types.Accessor.SparseType.IndicesType.ComponentTypeEnum.UNSIGNED_SHORT:
                        SparseIndices = new TypedArrayStorageFromBufferView<ushort>(
                            store, indices.BufferView,
                            indices.ByteOffset,
                            indices.ComponentType.SizeInBytes(),
                            1, // must be scalar
                            sparse.Count
                            ).CastTo<uint>();
                        break;

                    case Types.Accessor.SparseType.IndicesType.ComponentTypeEnum.UNSIGNED_INT:
                        SparseIndices = new TypedArrayStorageFromBufferView<uint>(
                            store, indices.BufferView,
                            indices.ByteOffset,
                            indices.ComponentType.SizeInBytes(),
                            1, // must be scalar
                            sparse.Count
                            );
                        break;
                }

                var values = sparse.Values;
                SparseValues = new TypedArrayStorageFromBufferView<T>(
                    store, values.BufferView,
                    values.ByteOffset,
                    accessor.ComponentType.SizeInBytes(),
                    accessor.Type.NumOfComponents(),
                    sparse.Count);
            }
        }

        public IEnumerable<T> GetEnumerable()
        {
            var denseArrayView = DenseView != null ? DenseView.GetEnumerable() : null;
            var sparseIndicesArrayView = SparseIndices != null ? SparseIndices.GetEnumerable() : null;
            var sparseValuesArrayView = SparseValues != null ? SparseValues.GetEnumerable() : null;

            using (var sparseIndicesIter = sparseIndicesArrayView != null ? sparseIndicesArrayView.GetEnumerator() : null)
            {
                var nextIndex = uint.MaxValue;
                if (sparseIndicesIter != null)
                {
                    sparseIndicesIter.MoveNext();
                    nextIndex = sparseIndicesIter.Current;
                }

                var sparseIndex = 0;
                for (int i = 0; i < Length; ++i)
                {
                    var v = default(T);

                    if (denseArrayView != null)
                    {
                        v = denseArrayView.ElementAt(i);
                    }

                    if (i == nextIndex)
                    {
                        v = sparseValuesArrayView.ElementAt(sparseIndex);
                        ++sparseIndex;

                        // Set uint.MaxValue when the iterator reached to end to prevent matching for condition.
                        nextIndex = sparseIndicesIter.MoveNext() ? sparseIndicesIter.Current : uint.MaxValue;
                    }

                    yield return v;
                }
            }
        }
    }

    public sealed class TypedBuffer
    {
        public ResourcesStore Store { get; }
        public Types.Accessor Accessor { get; }

        public TypedBuffer(ResourcesStore store, Types.Accessor accessor)
        {
            Store = store;
            Accessor = accessor;
        }

        public TypedArrayEntity<T> GetEntity<T>() where T : struct
        {
            // TODO: Type check for safety
            return new TypedArrayEntity<T>(Store, Accessor);
        }

        public IEnumerable<U> GetPrimitivesAsCasted<U>() where U : struct
        {
            if (Accessor.Type != Types.Accessor.TypeEnum.Scalar)
            {
                throw new InvalidOperationException("Type must be Scalar: Actual = " + Accessor.Type);
            }

            switch (Accessor.ComponentType)
            {
                case Types.Accessor.ComponentTypeEnum.BYTE:
                    return GetEntity<sbyte>()
                        .GetEnumerable()
                        .Select(x => (U)Convert.ChangeType(x, typeof(U)));

                case Types.Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                    return GetEntity<byte>()
                        .GetEnumerable()
                        .Select(x => (U)Convert.ChangeType(x, typeof(U)));

                case Types.Accessor.ComponentTypeEnum.SHORT:
                    return GetEntity<short>()
                        .GetEnumerable()
                        .Select(x => (U)Convert.ChangeType(x, typeof(U)));

                case Types.Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                    return GetEntity<ushort>()
                        .GetEnumerable()
                        .Select(x => (U)Convert.ChangeType(x, typeof(U)));

                case Types.Accessor.ComponentTypeEnum.UNSIGNED_INT:
                    return GetEntity<uint>()
                        .GetEnumerable()
                        .Select(x => (U)Convert.ChangeType(x, typeof(U)));

                case Types.Accessor.ComponentTypeEnum.FLOAT:
                    return GetEntity<float>()
                        .GetEnumerable()
                        .Select(x => (U)Convert.ChangeType(x, typeof(U)));

                default:
                    throw new InvalidOperationException("Unexpected ComponentType: Actual = " + Accessor.ComponentType);
            }
        }
    }
}
