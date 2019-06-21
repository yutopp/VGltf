//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using NUnit.Framework;
using System.Linq;

namespace VGltf.UnitTests.ModelTester
{
    public class ShinchokuRoboTester : IModelTester
    {
        public void TestModel(ResourcesStore store)
        {
            var gltf = store.Gltf;

            Assert.That(gltf.Scene, Is.EqualTo(0)); // Alicia model has no value, but migrated by VGltf;

            var buf0 = store.GetOrLoadBufferResourceAt(0);

            Assert.AreEqual(7, gltf.Images.Count);
            for (var i = 0; i < gltf.Images.Count; ++i)
            {
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
                Assert.AreEqual(0, img.BufferView);

                var r = store.GetOrLoadImageResourceAt(img.BufferView.Value);

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
                Assert.AreEqual(1, img.BufferView);

                var r = store.GetOrLoadImageResourceAt(img.BufferView.Value);

                var imageBytes = new byte[r.Data.Count];
                Array.Copy(r.Data.Array, r.Data.Offset, imageBytes, 0, r.Data.Count);

                Assert.AreEqual(65934, imageBytes.Count());
                Assert.That(imageBytes.Take(8), Is.EquivalentTo(new byte[] {
                                        // Header of PNG
                                        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
                                    }));
            }

            Assert.That(gltf.ExtensionsUsed, Is.EquivalentTo(new string[] { "KHR_materials_unlit", "VRM" }));
        }
    }
}
