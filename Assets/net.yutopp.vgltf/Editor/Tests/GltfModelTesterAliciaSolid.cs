//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using NUnit.Framework;

namespace VGltf.UnitTests.ModelTester
{
    public class AliciaSolidTester : IModelTester
    {
        public void TestModel(ResourcesStore store)
        {
            var gltf = store.Gltf;

            Assert.That(gltf.Scene, Is.EqualTo(0)); // Alicia model has no value, but migrated by VGltf;
            Assert.That(gltf.RootNodesIndices, Is.EquivalentTo(new int[] {
                                    0, 1, 114, 115, 116, 117, 118, 119, 120,
                                }));

            var buf0 = store.GetOrLoadBufferResourceAt(0);

            Assert.AreEqual(7, gltf.Images.Count);
            for (var i = 0; i < gltf.Images.Count; ++i)
            {
                var img = gltf.Images[i];
                var imgResN = store.GetOrLoadImageResourceAt(i);
            }

            Assert.AreEqual(248, gltf.Accessors.Count);
            for (var i = 0; i < gltf.Accessors.Count; ++i)
            {
                var typedBuffer = store.GetOrLoadTypedBufferByAccessorIndex(i);
            }

            Assert.AreEqual(7, gltf.Meshes.Count);

            // Test eyes mesh
            {
                var mesh = gltf.Meshes[2];
                Assert.That(mesh.Name, Is.EqualTo("eye.baked"));

                Assert.That(mesh.Primitives.Count, Is.EqualTo(1));

                var attr = mesh.Primitives[0].Attributes;
                Assert.That(attr[Types.Mesh.PrimitiveType.AttributeName.POSITION], Is.EqualTo(17));
                Assert.That(attr[Types.Mesh.PrimitiveType.AttributeName.NORMAL], Is.EqualTo(18));
                Assert.That(attr[Types.Mesh.PrimitiveType.AttributeName.TANGENT], Is.EqualTo(19));
                Assert.That(attr[Types.Mesh.PrimitiveType.AttributeName.TEXCOORD_0], Is.EqualTo(20));
                Assert.That(attr[Types.Mesh.PrimitiveType.AttributeName.JOINTS_0], Is.EqualTo(22));
                Assert.That(attr[Types.Mesh.PrimitiveType.AttributeName.WEIGHTS_0], Is.EqualTo(21));
            }

            Assert.AreEqual(7, gltf.Skins.Count);

            // Test eyes mesh
            {
                var skin = gltf.Skins[2];
                Assert.That(skin.InverseBindMatrices, Is.EqualTo(243));
                Assert.That(skin.Joints, Is.EqualTo(new int[] {
                                        37, 40, 38, 39,
                                    }));
                Assert.That(skin.Skeleton, Is.EqualTo(null));

                {
                    var node = gltf.Nodes[37]; // The eye is affected by this node
                    Assert.That(node.Name, Is.EqualTo("eye_L"));
                    Assert.That(node.Translation, Is.EquivalentTo(new float[]{
                                            -0.0276441593f, 0.0464049615f, -0.01160347f,
                                        }));
                }
            }

            // Assert.That(gltf.ExtensionsUsed, Is.EquivalentTo( new string[]{} ));
        }
    }
}
