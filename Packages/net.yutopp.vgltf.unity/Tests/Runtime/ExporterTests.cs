//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;

namespace VGltf.Unity.UnitTests
{
    public class ExporterTests
    {
        [Test]
        public void MeshExportTest()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

            using (var exporter = new Exporter())
            {
                exporter.ExportGameObjectAsScene(go);

                var gltfContainer = exporter.IntoGlbContainer();
                var store = new ResourcesStore(gltfContainer, new ResourceLoaderFromEmbedOnly());

                var gltf = gltfContainer.Gltf;

                Assert.AreEqual(0, gltf.Scene);
                Assert.AreEqual(1, gltf.Scenes.Count);

                var scene0 = gltf.Scenes[0];
                // TODO:

                Assert.AreEqual(1, gltf.Meshes.Count);

                var mesh0 = gltf.Meshes[0];
                Assert.AreEqual(null, mesh0.Weights);

                var prims = mesh0.Primitives;
                Assert.AreEqual(1, prims.Count);

                var prim0 = prims[0];
                Assert.AreEqual(null, prim0.Targets);
                Assert.AreEqual(0, prim0.Material);
                Assert.AreEqual(4, prim0.Indices);

                var prim0Accessor = gltf.Accessors[prim0.Indices.Value];
                Assert.AreEqual(36, prim0Accessor.Count); // 2(poly per faces) * 3(tri) * 6(faces)

                var prim0Buffer = store.GetOrLoadTypedBufferByAccessorIndex(prim0.Indices.Value);
                var prim0BufferEntity = prim0Buffer.GetEntity<uint, uint>((xs, i) => xs[i]);
                Assert.AreEqual(36, prim0BufferEntity.Length);
                Assert.That(prim0BufferEntity.AsArray().Take(6), Is.EquivalentTo(new int[] {
                    0, 2, 3, 0, 3, 1,
                }));

                var prim0Attr = prim0.Attributes;
                var prim0Position = prim0Attr["POSITION"];
                Assert.NotNull(prim0Position);
            }
        }
    }
}
