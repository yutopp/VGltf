//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using VJson;
using VJson.Schema;

namespace VGltf.Types
{
    // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/accessor.schema.json
    [JsonSchema(Title = "Accessor",
                Description = "A typed view into a bufferView.  A bufferView contains raw binary data.  An accessor provides a typed view into a bufferView or a subset of a bufferView similar to how WebGL's `vertexAttribPointer()` defines an attribute in a buffer.")]
    // TODO: support all of
    public class Accessor
    {
        // TODO: allOf": [ { "$ref": "glTFid.schema.json"} ]
        [JsonField(Name = "BufferView")]
        public int BufferView; // TODO: ignorable

        [JsonField(Name = "byteOffset")]
        [JsonSchema(Minimum = 0)]
        [JsonSchemaDependencies(new string[] { "bufferView" })]
        public int ByteOffset = 0;

        [JsonField(Name = "componentType")]
        [JsonSchemaRequired]
        public int ComponentType; // TODO enum

        [JsonField(Name = "normalized")]
        public bool Normalized = false;

        [JsonField(Name = "count")]
        [JsonSchema(Minimum = 1)]
        [JsonSchemaRequired]
        public int Count;

        [JsonField(Name = "type")]
        [JsonSchemaRequired]
        public string Type; // TODO enum

        [JsonField(Name = "max")]
        [JsonSchema(MinItems = 1, MaxItems = 16)]
        public float[] Max; // TODO: ignorable

        [JsonField(Name = "min")]
        [JsonSchema(MinItems = 1, MaxItems = 16)]
        public float[] Min; // TODO: ignorable

        [JsonField(Name = "sparse")]
        // TODO:"allOf": [ { "$ref": "accessor.sparse.schema.json" } ],
        public object Sparse; // TODO: ignorable

        [JsonField(Name = "name")]
        public object Name; // TODO: ignorable

        [JsonField(Name = "extensions")]
        public object Extensions; // TODO: ignorable

        [JsonField(Name = "extras")]
        public object extras; // TODO: ignorable
    }
}
