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
    using VJson.Schema;

    public class GltfContainerTests
    {
        // For testing purpose
        struct Vec2
        {
            float x;
            float y;

            public Vec2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public override string ToString()
            {
                return string.Format("{{{0}, {1}}}", x, y);
            }
        }

        struct Vec3
        {
            float x;
            float y;
            float z;

            public Vec3(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public override string ToString()
            {
                return string.Format("{{{0}, {1}, {2}}}", x, y, z);
            }
        }

        [Test]
        [TestCaseSource("GltfArgs")]
        public void FromGltfTest(string[] modelPath, Action<Types.Gltf, Glb.StoredBuffer> assertGltf)
        {
            var path = modelPath.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var c = GltfContainer.FromGltf(fs);

                var schema = VJson.Schema.JsonSchemaAttribute.CreateFromClass<Types.Gltf>();
                var ex = schema.Validate(c.Gltf);
                Assert.Null(ex);

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

                var schema = VJson.Schema.JsonSchemaAttribute.CreateFromClass<Types.Gltf>();
                var ex = schema.Validate(c.Gltf);
                Assert.Null(ex);

                assertGltf(c.Gltf, c.Buffer);
            }
        }

        public static object[] GltfArgs = {
            new object[] {
                // See: https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/master/2.0/SimpleSparseAccessor/screenshot/simpleSparseAccessorStructure.png
                new string[] {"SimpleSparseAccessor", "glTF-Embedded", "SimpleSparseAccessor.gltf"},
                new Action<Types.Gltf, Glb.StoredBuffer>(
                    (gltf, buffer) => {
                        var store = new ResourcesStore(gltf, buffer, new ResourceLoaderFromStorage());

                        Assert.AreEqual(2, gltf.Accessors.Count);

                        // indicesView
                        {
                            var typedBuffer = store.GetOrLoadTypedBufferByAccessorIndex(0);
                            Assert.AreEqual(Types.Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                                            typedBuffer.Accessor.ComponentType);
                            Assert.AreEqual(Types.Accessor.TypeEnum.Scalar, typedBuffer.Accessor.Type);

                            var entiry = typedBuffer.GetEntity<ushort>();
                            Assert.AreEqual(36, entiry.Length);
                        }

                        // positionView
                        {
                            var typedBuffer = store.GetOrLoadTypedBufferByAccessorIndex(1);
                            Assert.AreEqual(Types.Accessor.ComponentTypeEnum.FLOAT,
                                            typedBuffer.Accessor.ComponentType);
                            Assert.AreEqual(Types.Accessor.TypeEnum.Vec3, typedBuffer.Accessor.Type);

                            Assert.NotNull(typedBuffer.Accessor.Sparse);
                            Assert.AreEqual(3, typedBuffer.Accessor.Sparse.Count);

                            var entiry = typedBuffer.GetEntity<Vec3>();
                            Assert.AreEqual(14, entiry.Length);

                            // For indices
                            var indices = entiry.SparseIndices;
                            Assert.AreEqual(3, indices.GetEnumerable().Count());
                            Assert.That(indices.GetEnumerable(), Is.EquivalentTo(new int[] { 8, 10, 12 }));

                            // For values
                            var values = entiry.SparseValues;
                            Assert.AreEqual(3, values.GetEnumerable().Count());
                            Assert.That(values.GetEnumerable(), Is.EquivalentTo(new Vec3[] {
                                        new Vec3(1.0f, 2.0f, 0.0f),
                                        new Vec3(3.0f, 3.0f, 0.0f),
                                        new Vec3(5.0f, 4.0f, 0.0f),
                                    }));

                            // For merged view
                            Assert.That(entiry.GetEnumerable(), Is.EquivalentTo(new Vec3[] {
                                        new Vec3(0.0f, 0.0f, 0.0f),
                                        new Vec3(1.0f, 0.0f, 0.0f),
                                        new Vec3(2.0f, 0.0f, 0.0f),
                                        new Vec3(3.0f, 0.0f, 0.0f),
                                        new Vec3(4.0f, 0.0f, 0.0f),
                                        new Vec3(5.0f, 0.0f, 0.0f),
                                        new Vec3(6.0f, 0.0f, 0.0f),
                                        new Vec3(0.0f, 1.0f, 0.0f),
                                        new Vec3(1.0f, 2.0f, 0.0f), // 8 (sparse)
                                        new Vec3(2.0f, 1.0f, 0.0f),
                                        new Vec3(3.0f, 3.0f, 0.0f), // 10 (sparse)
                                        new Vec3(4.0f, 1.0f, 0.0f),
                                        new Vec3(5.0f, 4.0f, 0.0f), // 12 (sparse)
                                        new Vec3(6.0f, 1.0f, 0.0f),
                                    }));
                        }
                    })
            },

            new object[] {
                // See: https://github.com/KhronosGroup/glTF-Sample-Models/tree/master/2.0/BoxTextured
                new string[] {"BoxTextured", "glTF-Embedded", "BoxTextured.gltf"},
                new Action<Types.Gltf, Glb.StoredBuffer>(
                    (gltf, buffer) => {
                        var store = new ResourcesStore(gltf, buffer, new ResourceLoaderFromStorage());

                        Assert.NotNull(gltf.Scene);
                        var rootNodes = gltf.RootNodes.ToList();

                        Assert.AreEqual(1, rootNodes.Count);

                        var node = rootNodes[0];
                        Assert.That(node.Matrix, Is.EquivalentTo(new float[] {
                                                    1.0f, 0.0f, 0.0f, 0.0f,
                                                    0.0f, 0.0f,-1.0f, 0.0f,
                                                    0.0f, 1.0f, 0.0f, 0.0f,
                                                    0.0f, 0.0f, 0.0f, 1.0f,
                                }));
                        Assert.That(node.Children, Is.EquivalentTo(new int[] { 1 }));

                        var childNode = gltf.Nodes[node.Children[0]];
                        Assert.NotNull(childNode);

                        Assert.AreEqual(0, childNode.Mesh);

                        var mesh = gltf.Meshes[childNode.Mesh.Value];
                        Assert.AreEqual("Mesh", mesh.Name);
                        Assert.AreEqual(1, mesh.Primitives.Count);

                        var primitive = mesh.Primitives[0];
                        Assert.AreEqual(1, primitive.Attributes["NORMAL"]);
                        Assert.AreEqual(2, primitive.Attributes["POSITION"]);
                        Assert.AreEqual(3, primitive.Attributes["TEXCOORD_0"]);
                        Assert.AreEqual(0, primitive.Indices);
                        Assert.AreEqual(Types.Mesh.PrimitiveType.ModeEnum.TRIANGLES, primitive.Mode);
                        Assert.AreEqual(0, primitive.Material);

                        // Accesses
                        // index 1
                        var normal = store.GetOrLoadTypedBufferByAccessorIndex(primitive.Attributes["NORMAL"]);
                        Assert.AreEqual(24, normal.GetEntity<Vec3>().GetEnumerable().Count());

                        // index 2
                        var position = store.GetOrLoadTypedBufferByAccessorIndex(primitive.Attributes["POSITION"]);
                        Assert.AreEqual(24, position.GetEntity<Vec3>().GetEnumerable().Count());

                        // index 3
                        var texCoord0 = store.GetOrLoadTypedBufferByAccessorIndex(primitive.Attributes["TEXCOORD_0"]);
                        Assert.AreEqual(24, texCoord0.GetEntity<Vec2>().GetEnumerable().Count());

                        // index 0
                        var indicies = store.GetOrLoadTypedBufferByAccessorIndex(primitive.Indices.Value);
                        Assert.AreEqual(36, indicies.GetPrimitivesAsCasted<int>().Count());

                        // Materials
                        // index0
                        var material = gltf.Materials[primitive.Material.Value];
                        Assert.AreEqual("Texture", material.Name);

                        Assert.NotNull(material.PbrMetallicRoughness);
                        Assert.NotNull(material.PbrMetallicRoughness.BaseColorTexture);
                        Assert.AreEqual(0, material.PbrMetallicRoughness.BaseColorTexture.Index);
                        Assert.AreEqual(0.0f, material.PbrMetallicRoughness.MetallicFactor);

                        // Textures
                        var texture = gltf.Textures[material.PbrMetallicRoughness.BaseColorTexture.Index];
                        Assert.AreEqual(0, texture.Sampler);
                        Assert.AreEqual(0, texture.Source);

                        // Samplers
                        var sampler = gltf.Samplers[texture.Sampler.Value];

                        // Images(source)
                        var imageResource = store.GetOrLoadImageResourceAt(texture.Source.Value);
                        Assert.AreEqual(23516, imageResource.Data.Count);

                        var imageBytes = new byte[imageResource.Data.Count];
                        Array.Copy(imageResource.Data.Array, imageResource.Data.Offset, imageBytes, 0, imageResource.Data.Count);

                        // Compare to the base texture image
                        var baseImagePath = new string[]{
                            "BoxTextured",
                            "glTF",
                            "CesiumLogoFlat.png"
                        }.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));
                        var baseImageBytes = File.ReadAllBytes(baseImagePath);

                        Assert.AreEqual(BitConverter.ToString(baseImageBytes),
                                        BitConverter.ToString(imageBytes));
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

                        //
                        {
                            var img = gltf.Images[0];
                            Assert.AreEqual("body", img.Name);
                            Assert.AreEqual(null, img.Uri);
                            Assert.AreEqual(0, img.bufferView);

                            var r = store.GetOrLoadImageResourceAt(img.bufferView.Value);

                            var imageBytes = new byte[r.Data.Count];
                            Array.Copy(r.Data.Array, r.Data.Offset, imageBytes, 0, r.Data.Count);

                            Assert.AreEqual(1821190, imageBytes.Count());
                            Assert.That(imageBytes.Take(8), Is.EquivalentTo(new byte[] {
                                        // Header of PNG
                                        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                                    }));
                        }

                        {
                            var img = gltf.Images[1];
                            Assert.AreEqual("eye", img.Name);
                            Assert.AreEqual(null, img.Uri);
                            Assert.AreEqual(1, img.bufferView);

                            var r = store.GetOrLoadImageResourceAt(img.bufferView.Value);

                            var imageBytes = new byte[r.Data.Count];
                            Array.Copy(r.Data.Array, r.Data.Offset, imageBytes, 0, r.Data.Count);

                            Assert.AreEqual(65934, imageBytes.Count());
                            Assert.That(imageBytes.Take(8), Is.EquivalentTo(new byte[] {
                                        // Header of PNG
                                        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                                    }));
                        }

                        Assert.That(gltf.ExtensionsUsed, Is.EquivalentTo( new string[]{ "KHR_materials_unlit", "VRM" } ));
                    }),
            },
        };
    }
}
