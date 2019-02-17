//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using VJson;
using VJson.Schema;

// Reference: https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/*
namespace VGltf.Types
{
    [JsonSchema(Id = "bufferView.schema.json")]
    public class BufferView
    {
        [JsonField(Name = "buffer")]
        // TODO: allOf": [ { "$ref": "glTFid.schema.json"} ]
        [JsonSchemaRequired]
        public int Buffer;

        [JsonField(Name = "byteOffset")]
        [JsonSchema(Minimum = 0)]
        public int ByteOffset = 0; // TODO: ignorable

        [JsonField(Name = "byteLength")]
        [JsonSchema(Minimum = 1)]
        [JsonSchemaRequired]
        public int ByteLength;

        [JsonField(Name = "byteStride")]
        [JsonSchema(Minimum = 4, Maximum = 252, MultipleOf = 4)]
        public int ByteStride; // TODO: ignorable

        [JsonField(Name = "target")]
        // TODO: enum
        public int Target; // TODO: ignorable

        [JsonField(Name = "name")]
        public object name; // TODO: ignorable

        [JsonField(Name = "extensions")]
        public object Extensions; // TODO: ignorable

        [JsonField(Name = "extras")]
        public object Extras; // TODO: ignorable
    }
}
