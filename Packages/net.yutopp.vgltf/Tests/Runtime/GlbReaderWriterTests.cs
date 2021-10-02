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

namespace VGltf.UnitTests
{
    public class GlbReaderWriterTests
    {
        [Test]
        [TestCaseSource("GlbArgs")]
        public void ReadWriteTest(string[] modelPath)
        {
            var path = modelPath.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));

            Glb.Header header0;
            Glb.Chunk c0Json;
            Glb.Chunk c0Buffer;
            using (var fs = StreamReaderFactory.CreateStream(path))
            using (var r0 = new Glb.Reader(fs))
            {
                header0 = r0.ReadHeader();
                c0Json = r0.ReadChunk();
                c0Buffer = r0.ReadChunk();
            }

            // re-export glb
            byte[] output;
            using (var fs = StreamReaderFactory.CreateStream(path))
            {
                var c = GltfContainer.FromGlb(fs);
                using (var ms = new MemoryStream())
                {
                    GltfContainer.ToGlb(ms, c);
                    output = ms.ToArray();
                }
            }

            using (var ms = new MemoryStream(output))
            using (var r1 = new Glb.Reader(ms))
            {
                var header1 = r1.ReadHeader();
                Assert.AreEqual(header0.Magic, header1.Magic);
                Assert.AreEqual(header0.Version, header1.Version);
                // Assert.AreEqual(header0.Length, header1.Length);

                var c1Json = r1.ReadChunk();
                // Assert.AreEqual(c0Json.ChunkLength, c1Json.ChunkLength);
                Assert.AreEqual(c0Json.ChunkType, c1Json.ChunkType);

                var c1Buffer = r1.ReadChunk();
                Assert.AreEqual(c0Buffer.ChunkLength, c1Buffer.ChunkLength);
                Assert.AreEqual(c0Buffer.ChunkType, c1Buffer.ChunkType);
            }
        }

        public static object[] GlbArgs = {
            new object[] {
                new string[] {"Alicia", "VRM", "AliciaSolid.vrm"},
            },

            new object[] {
                new string[] {"ShinchokuRobo", "shinchoku_robo.vrm"},
            },
        };
    }
}
