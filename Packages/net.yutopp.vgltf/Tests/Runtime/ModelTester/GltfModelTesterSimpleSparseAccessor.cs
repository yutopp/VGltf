//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Linq;
using NUnit.Framework;
using VGltf.UnitTests.Shims;

namespace VGltf.UnitTests.ModelTester
{
    using Vec3Float = Vec3<float>;

    // See: https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/SimpleSparseAccessor/screenshot/simpleSparseAccessorStructure.png
    public class SimpleSparseAccessorTester : IModelTester
    {
        public void TestModel(ResourcesStore store)
        {
            var gltf = store.Gltf;

            Assert.AreEqual(2, gltf.Accessors.Count);

            // indicesView
            {
                var typedBuffer = store.GetOrLoadTypedBufferByAccessorIndex(0);
                Assert.AreEqual(Types.Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                                typedBuffer.Accessor.ComponentType);
                Assert.AreEqual(Types.Accessor.TypeEnum.Scalar, typedBuffer.Accessor.Type);

                var entity = typedBuffer.GetEntity<ushort, ushort>(xs => xs[0]);
                Assert.AreEqual(36, entity.Length);
            }

            // positionView
            {
                var typedBuffer = store.GetOrLoadTypedBufferByAccessorIndex(1);
                Assert.AreEqual(Types.Accessor.ComponentTypeEnum.FLOAT,
                                typedBuffer.Accessor.ComponentType);
                Assert.AreEqual(Types.Accessor.TypeEnum.Vec3, typedBuffer.Accessor.Type);

                Assert.NotNull(typedBuffer.Accessor.Sparse);
                Assert.AreEqual(3, typedBuffer.Accessor.Sparse.Count);

                var entity = typedBuffer.GetEntity<float, Vec3Float>(Vec3Float.FromArray);
                Assert.AreEqual(14, entity.Length);

                // For indices
                var indices = entity.SparseIndices;
                Assert.AreEqual(3, indices.Count());
                Assert.That(indices, Is.EquivalentTo(new int[] { 8, 10, 12 }));

                // For values
                var values = entity.SparseValues.GetEnumerable().ToArray();
                Assert.AreEqual(3, values.Count());
                Assert.That(values, Is.EquivalentTo(new Vec3Float[] {
                                new Vec3Float(1.0f, 2.0f, 0.0f),
                                new Vec3Float(3.0f, 3.0f, 0.0f),
                                new Vec3Float(5.0f, 4.0f, 0.0f),
                            }));

                // For merged view
                var mergedValues = entity.GetEnumerable().ToArray();
                Assert.That(mergedValues, Is.EquivalentTo(new Vec3Float[] {
                                new Vec3Float(0.0f, 0.0f, 0.0f),
                                new Vec3Float(1.0f, 0.0f, 0.0f),
                                new Vec3Float(2.0f, 0.0f, 0.0f),
                                new Vec3Float(3.0f, 0.0f, 0.0f),
                                new Vec3Float(4.0f, 0.0f, 0.0f),
                                new Vec3Float(5.0f, 0.0f, 0.0f),
                                new Vec3Float(6.0f, 0.0f, 0.0f),
                                new Vec3Float(0.0f, 1.0f, 0.0f),
                                new Vec3Float(1.0f, 2.0f, 0.0f), // 8 (sparse)
                                new Vec3Float(2.0f, 1.0f, 0.0f),
                                new Vec3Float(3.0f, 3.0f, 0.0f), // 10 (sparse)
                                new Vec3Float(4.0f, 1.0f, 0.0f),
                                new Vec3Float(5.0f, 4.0f, 0.0f), // 12 (sparse)
                                new Vec3Float(6.0f, 1.0f, 0.0f),
                            }));
            }
        }
    }
}
