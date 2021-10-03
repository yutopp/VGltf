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
        //IEnumerable<T> GetEnumerable();
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class TypedArrayView<T> : ITypedView<T> where T : struct
    {
        readonly ArraySegment<byte> _buffer;
        readonly int _stride;

        readonly int _componentSize;
        readonly int _componentNum;
        readonly int _count;

        public TypedArrayView(
            ArraySegment<byte> buffer,
            int stride,
            int componentSize, // Size of primitives (e.g. int = 4)
            int componentNum,  // Number of primitives in compound values (e.g. VEC3 = 3)
            int count)         // Number of compound values (e.g. Number of VEC3)
        {
            // assert sizeof(T) == _componentSize;
            //
            // https://www.khronos.org/files/gltf20-reference-guide.pdf
            // e.g. stride = 12
            //             20 = 8 + (12 = stride)
            // 8   12  16  20  24  28
            // |---|---|   |---|---|
            // [ x | y ]   [ x | y ] : Vec2<float>
            //
            _buffer = buffer;
            _stride = stride;
            _componentSize = componentSize;
            _componentNum = componentNum;
            _count = count;
        }

        // TODO: improve performance
        public IEnumerable<U> GetCompositedEnumerable<U>(Func<T[], U> mapper)
        {
            var origin = new T[_componentNum];
            var gch = GCHandle.Alloc(origin, GCHandleType.Pinned);
            try
            {
                for (var i = 0; i < _count; ++i)
                {
                    var strideHeadOffset = i * _stride;

                    // All buffer data are little endian.
                    // See: https://github.com/KhronosGroup/glTF/blob/8e3810c01a04930a8c98b2d76232b63f4dab944f/specification/2.0/Specification.adoc#36-binary-data-storage
                    //
                    // TODO: If you use this library on machines which have other endianness, need to implement supporting that.
                    //

                    Marshal.Copy(_buffer.Array, _buffer.Offset + strideHeadOffset, gch.AddrOfPinnedObject(), _componentSize * _componentNum);
                    yield return mapper(origin);
                }
            }
            finally
            {
                gch.Free();
            }
        }
    }

    public sealed class TypedArrayStorageFromBufferView<T> : ITypedView<T> where T : struct
    {
        readonly TypedArrayView<T> _view;

        public TypedArrayStorageFromBufferView(ResourcesStore store,
                                               int bufferViewIndex,
                                               int byteOffset,
                                               int componentSize, // Size of primitives (e.g. int = 4)
                                               int componentNum,  // Number of primitives in compound values (e.g. VEC3 = 3)
                                               int count)         // Number of compound values (e.g. Number of VEC3)
        {
            var bufferView = store.Gltf.BufferViews[bufferViewIndex];
            var r = store.GetOrLoadBufferViewResourceAt(bufferViewIndex);

            //
            // https://www.khronos.org/files/gltf20-reference-guide.pdf
            // e.g. stride = 12
            //             20 = 8 + (12 = stride)
            // 8   12  16  20  24  28
            // |---|---|   |---|---|
            // [ x | y ]   [ x | y ] : Vec2<float>
            //
            var compoundSize = componentSize * componentNum; // e.g. Size in bytes sizeof(int * VEC3) = 4 * 3
            var stride = bufferView.ByteStride != null ? bufferView.ByteStride.Value : compoundSize;

            var buffer = new ArraySegment<byte>(r.Data.Array, r.Data.Offset + byteOffset, count * stride);

            _view = new TypedArrayView<T>(buffer, stride, componentSize, componentNum, count);
        }

        public IEnumerable<U> GetCompositedEnumerable<U>(Func<T[], U> mapper)
        {
            return _view.GetCompositedEnumerable<U>(mapper);
        }
    }

    public sealed class TypedArrayEntity<T> : ITypedView<T> where T : struct
    {
        public TypedArrayStorageFromBufferView<T> DenseView { get; }

        public uint[] SparseIndices { get; }
        public TypedArrayStorageFromBufferView<T> SparseValues { get; }

        public int Length { get; }
        readonly int _componentNum; // Number of primitives in compound values (e.g. VEC3 = 3)

        public TypedArrayEntity(ResourcesStore store, Accessor accessor)
        {
            Length = accessor.Count;
            _componentNum = accessor.Type.NumOfComponents();

            if (accessor.BufferView != null)
            {
                DenseView = new TypedArrayStorageFromBufferView<T>(
                    store,
                    accessor.BufferView.Value,
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
                            store,
                            indices.BufferView,
                            indices.ByteOffset,
                            indices.ComponentType.SizeInBytes(),
                            1, // must be scalar
                            sparse.Count
                            )
                            .GetCompositedEnumerable(xs => (uint)xs[0])
                            .ToArray();
                        break;

                    case Types.Accessor.SparseType.IndicesType.ComponentTypeEnum.UNSIGNED_SHORT:
                        SparseIndices = new TypedArrayStorageFromBufferView<ushort>(
                            store,
                            indices.BufferView,
                            indices.ByteOffset,
                            indices.ComponentType.SizeInBytes(),
                            1, // must be scalar
                            sparse.Count
                            )
                            .GetCompositedEnumerable(xs => (uint)xs[0])
                            .ToArray();
                        break;

                    case Types.Accessor.SparseType.IndicesType.ComponentTypeEnum.UNSIGNED_INT:
                        SparseIndices = new TypedArrayStorageFromBufferView<uint>(
                            store,
                            indices.BufferView,
                            indices.ByteOffset,
                            indices.ComponentType.SizeInBytes(),
                            1, // must be scalar
                            sparse.Count
                            )
                            .GetCompositedEnumerable(xs => (uint)xs[0])
                            .ToArray();
                        break;
                }

                var values = sparse.Values;
                SparseValues = new TypedArrayStorageFromBufferView<T>(
                    store,
                    values.BufferView,
                    values.ByteOffset,
                    accessor.ComponentType.SizeInBytes(),
                    accessor.Type.NumOfComponents(),
                    sparse.Count);
            }
        }

        public IEnumerable<U> GetCompositedEnumerable<U>(Func<T[], U> mapper)
        {
            var denseArrayView = DenseView != null ? DenseView.GetCompositedEnumerable(mapper) : null;
            var sparseValuesArrayView = SparseValues != null ? SparseValues.GetCompositedEnumerable(mapper) : null;

            var sparseTargetIndex = uint.MaxValue;
            if (SparseIndices != null)
            {
                sparseTargetIndex = SparseIndices[0]; // assume that SparseIndices has at least 1 items
            }

            var defaultEmptyValue = new T[_componentNum];
            var sparseIndex = 0;
            // e.g. Vec3[0] ... Vec[i] ... Vec[Length-1]
            for (int i = 0; i < Length; ++i)
            {
                var v = default(U);

                if (denseArrayView != null)
                {
                    v = denseArrayView.ElementAt(i);
                }

                if (i == sparseTargetIndex)
                {
                    v = sparseValuesArrayView.ElementAt(sparseIndex);
                    ++sparseIndex;

                    // Set uint.MaxValue when the iterator reached to end to prevent matching for condition.
                    sparseTargetIndex = sparseIndex < SparseIndices.Length ? SparseIndices[sparseIndex] : uint.MaxValue;
                }

                yield return v;
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
                        .GetCompositedEnumerable(xs => (U)Convert.ChangeType(xs[0], typeof(U)));

                case Types.Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
                    return GetEntity<byte>()
                        .GetCompositedEnumerable(xs => (U)Convert.ChangeType(xs[0], typeof(U)));

                case Types.Accessor.ComponentTypeEnum.SHORT:
                    return GetEntity<short>()
                        .GetCompositedEnumerable(xs => (U)Convert.ChangeType(xs[0], typeof(U)));

                case Types.Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
                    return GetEntity<ushort>()
                        .GetCompositedEnumerable(xs => (U)Convert.ChangeType(xs[0], typeof(U)));

                case Types.Accessor.ComponentTypeEnum.UNSIGNED_INT:
                    return GetEntity<uint>()
                        .GetCompositedEnumerable(xs => (U)Convert.ChangeType(xs[0], typeof(U)));

                case Types.Accessor.ComponentTypeEnum.FLOAT:
                    return GetEntity<float>()
                        .GetCompositedEnumerable(xs => (U)Convert.ChangeType(xs[0], typeof(U)));

                default:
                    throw new InvalidOperationException("Unexpected ComponentType: Actual = " + Accessor.ComponentType);
            }
        }
    }
}
