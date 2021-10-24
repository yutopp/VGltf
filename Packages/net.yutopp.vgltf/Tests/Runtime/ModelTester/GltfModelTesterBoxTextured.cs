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
using VGltf.UnitTests.Shims;

namespace VGltf.UnitTests.ModelTester
{
    using Vec2Float = Vec2<float>;
    using Vec3Float = Vec3<float>;

    // See: https://github.com/KhronosGroup/glTF-Sample-Models/tree/master/2.0/BoxTextured
    public class BoxTexturedTester : IModelTester
    {
        public void TestModel(ResourcesStore store)
        {
            var gltf = store.Gltf;

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
            Assert.AreEqual(24, normal.GetEntity<float, Vec3Float>(Vec3Float.FromArray).AsArray().Length);

            // index 2
            var position = store.GetOrLoadTypedBufferByAccessorIndex(primitive.Attributes["POSITION"]);
            Assert.AreEqual(24, position.GetEntity<float, Vec3Float>(Vec3Float.FromArray).AsArray().Length);

            // index 3
            var texCoord0 = store.GetOrLoadTypedBufferByAccessorIndex(primitive.Attributes["TEXCOORD_0"]);
            Assert.AreEqual(24, texCoord0.GetEntity<float, Vec2Float>(Vec2Float.FromArray).AsArray().Length);

            // index 0
            var indicies = store.GetOrLoadTypedBufferByAccessorIndex(primitive.Indices.Value);
            Assert.AreEqual(36, indicies.GetPrimitivesAsInt().Count());

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
            var imagePath = new string[] {
                "BoxTextured",
                "glTF",
                "CesiumLogoFlat.png"
            }.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));

            byte[] baseImageBytes;
            using (var fs = StreamReaderFactory.CreateStream(imagePath))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                baseImageBytes = ms.ToArray();
            }
            Assert.AreEqual(BitConverter.ToString(baseImageBytes),
                            BitConverter.ToString(imageBytes));
        }
    }
}
