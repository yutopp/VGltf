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
using VJson.Schema;

namespace VGltf.UnitTests
{
    public class GltfContainerTests
    {
        [Test]
        [TestCaseSource("GltfArgs")]
        public void FromGltfTest(string[] modelPath, ModelTester.IModelTester tester)
        {
            var path = modelPath.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));
            using (var fs = StreamReaderFactory.CreateStream(path))
            {
                var c = GltfContainer.FromGltf(fs);

                var schema = VJson.Schema.JsonSchema.CreateFromType<Types.Gltf>(c.JsonSchemas);
                var ex = schema.Validate(c.Gltf, c.JsonSchemas);
                Assert.Null(ex);

                var fullStorageDir = Directory.GetParent(path).FullName;
                var relStorageDir = FilePath.GetRelativePath(fullStorageDir, Directory.GetCurrentDirectory());
                var loader = new Shims.ResourceLoaderFromAssets(relStorageDir);

                var store = new ResourcesStore(c, loader);
                tester.TestModel(store);
            }
        }

        [Test]
        [TestCaseSource("GlbArgs")]
        public void FromGlbTest(string[] modelPath, ModelTester.IModelTester tester)
        {
            var path = modelPath.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));
            using (var fs = StreamReaderFactory.CreateStream(path))
            {
                var c = GltfContainer.FromGlb(fs);

                var schema = VJson.Schema.JsonSchema.CreateFromType<Types.Gltf>(c.JsonSchemas);
                var ex = schema.Validate(c.Gltf, c.JsonSchemas);
                Assert.Null(ex);

                var loader = new ResourceLoaderFromEmbedOnly(); // Glb files should be packed.

                var store = new ResourcesStore(c, loader);
                tester.TestModel(store);
            }
        }

        public static object[] GltfArgs = {
            new object[] {
                new string[] {"SimpleSparseAccessor", "glTF-Embedded", "SimpleSparseAccessor.gltf"},
                new ModelTester.SimpleSparseAccessorTester(),
            },

            new object[] {
                new string[] {"BoxTextured", "glTF-Embedded", "BoxTextured.gltf"},
                new ModelTester.BoxTexturedTester(),
            },

            new object[] {
                new string[] {"BoxTextured", "glTF", "BoxTextured.gltf"},
                new ModelTester.BoxTexturedTester(),
            },

            new object[] {
                new string[] {"RiggedSimple", "glTF-Embedded", "RiggedSimple.gltf"},
                new ModelTester.RiggedSimpleTester(),
            },
        };

        public static object[] GlbArgs = {
            new object[] {
                new string[] {"Alicia", "VRM", "AliciaSolid.vrm"},
                new ModelTester.AliciaSolidTester(),
            },

            new object[] {
                new string[] {"ShinchokuRobo", "shinchoku_robo.vrm"},
                new ModelTester.ShinchokuRoboTester(),
            },
        };
    }
}
