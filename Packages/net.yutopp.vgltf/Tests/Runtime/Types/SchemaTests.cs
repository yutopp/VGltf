//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using NUnit.Framework;

namespace VGltf.Types.UnitTests
{
    public class SchemaTests
    {
        [Test]
        [TestCaseSource("ValuesArgs")]
        public void GLTFTest(Type ty, string expectedSchema)
        {
            var serializer = new VJson.JsonSerializer(typeof(VJson.Schema.JsonSchemaAttribute));
            var schema = serializer.Serialize(VJson.Schema.JsonSchema.CreateFromType(ty));

            //Assert.AreEqual(expectedSchema, schema);
        }

        public static object[] ValuesArgs = {
            new object[] {
                typeof(Gltf),
                @"""
"""
            },
        };
    }
}
