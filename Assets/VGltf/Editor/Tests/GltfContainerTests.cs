//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace VGltf.UnitTests
{
    public class GltfContainerTests
    {
        [Test]
        [TestCaseSource("GltfArgs")]
        public void FromGltfTest(string[] modelPath, Action<Types.Gltf, Glb.StoredBuffer> assertGltf)
        {
            var path = modelPath.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var c = GltfContainer.FromGltf(fs);
                assertGltf(c.Gltf, c.Buffer);
            }
        }

        [Test]
        [TestCaseSource("GlbArgs")]
        public void FromGlbTest(string[] modelPath, Action<Types.Gltf, Glb.StoredBuffer> assertGltf)
        {
            var path = modelPath.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var c = GltfContainer.FromGlb(fs);
                assertGltf(c.Gltf, c.Buffer);
            }
        }

        public static object[] GltfArgs = {
            new object[] {
                // See: https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/SimpleSparseAccessor/screenshot/simpleSparseAccessorStructure.png
                new string[] {"SimpleSparseAccessor", "SimpleSparseAccessor.gltf"},
                new Action<Types.Gltf, Glb.StoredBuffer>(
                    (gltf, buffer) => {
                        var store = new ResourcesStore(gltf, buffer, new ResourceLoaderFromStorage());

                        Assert.AreEqual(2, gltf.Accessors.Count);

                        // indicesView
                        {
                            var typedBuffer = store.GetOrLoadTypedBufferByAccessorIndex(0);
                            Assert.AreEqual(Types.Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                                            typedBuffer.Accessor.ComponentType);

                            var view = typedBuffer.GetUnsignedShortView();
                            Assert.AreEqual(36 * 1 /* 1(Scalar) */, view.Length);
                        }

                        // positionView
                        {
                            var typedBuffer = store.GetOrLoadTypedBufferByAccessorIndex(1);
                            Assert.AreEqual(Types.Accessor.ComponentTypeEnum.FLOAT,
                                            typedBuffer.Accessor.ComponentType);

                            var view = typedBuffer.GetFloatView();
                            Assert.AreEqual(14 * 3 /* 3(VEC3) */, view.Length);

                            Assert.NotNull(typedBuffer.Accessor.Sparse);
                            Assert.AreEqual(3, typedBuffer.Accessor.Sparse.Count);

                            // For indices
                            var indices = view.SparceIndices;
                            Assert.AreEqual(3, indices.GetEnumerable().Count());
                            Assert.That(indices.GetEnumerable(), Is.EquivalentTo(new int[] { 8, 10, 12 }));

                            // For values
                            var values = view.SparceValues;
                            Assert.AreEqual(3 * 3 /* 3(VEC3) */, values.GetEnumerable().Count());
                            Assert.That(values.GetEnumerable(), Is.EquivalentTo(new float[] {
                                        1.0f, 2.0f, 0.0f,
                                        3.0f, 3.0f, 0.0f,
                                        5.0f, 4.0f, 0.0f
                                    }));

                            // For merged view
                            Assert.That(view.GetEnumerable(), Is.EquivalentTo(new float[] {
                                        0.0f, 0.0f, 0.0f,
                                        1.0f, 0.0f, 0.0f,
                                        2.0f, 0.0f, 0.0f,
                                        3.0f, 0.0f, 0.0f,
                                        4.0f, 0.0f, 0.0f,
                                        5.0f, 0.0f, 0.0f,
                                        6.0f, 0.0f, 0.0f,
                                        0.0f, 1.0f, 0.0f,
                                        1.0f, 2.0f, 0.0f, // 8 (sparse)
                                        2.0f, 1.0f, 0.0f,
                                        3.0f, 3.0f, 0.0f, // 10 (sparse)
                                        4.0f, 1.0f, 0.0f,
                                        5.0f, 4.0f, 0.0f, // 12 (sparse)
                                        6.0f, 1.0f, 0.0f
                                    }));
                        }
                    })
            },
        };

        public static object[] GlbArgs = {
            new object[] {
                new string[] {"Alicia", "VRM", "AliciaSolid.vrm"},
                new Action<Types.Gltf, Glb.StoredBuffer>(
                    (gltf, buffer) => {
                        var store = new ResourcesStore(gltf, buffer, new ResourceLoaderFromStorage());

                        var buf0 = store.GetOrLoadBufferResourceAt(0);

                        Assert.AreEqual(7, gltf.Images.Count);
                        for(var i=0; i<gltf.Images.Count; ++i) {
                            var img = gltf.Images[i];
                            var imgResN = store.GetOrLoadImageResourceAt(i);
                        }

                        Assert.AreEqual(248, gltf.Accessors.Count);
                        for(var i=0; i<gltf.Accessors.Count; ++i) {
                            var typedBuffer = store.GetOrLoadTypedBufferByAccessorIndex(i);
                        }

                        // Assert.That(gltf.ExtensionsUsed, Is.EquivalentTo( new string[]{} ));
                    }),
            },

            new object[] {
                new string[] {"ShinchokuRobo", "shinchoku_robo.vrm"},
                new Action<Types.Gltf, Glb.StoredBuffer>(
                    (gltf, buffer) => {
                        var store = new ResourcesStore(gltf, buffer, new ResourceLoaderFromStorage());

                        var buf0 = store.GetOrLoadBufferResourceAt(0);

                        Assert.AreEqual(7, gltf.Images.Count);
                        for(var i=0; i<gltf.Images.Count; ++i) {
                            var img = gltf.Images[i];
                            var imgResN = store.GetOrLoadImageResourceAt(i);

                            // imgN.Data
                            // Console.WriteLine(img.Name);
                        }

                        Assert.That(gltf.ExtensionsUsed, Is.EquivalentTo( new string[]{ "KHR_materials_unlit", "VRM" } ));
                    }),
            },
        };
    }
}
