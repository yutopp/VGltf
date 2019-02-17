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
    [JsonSchema(Id = "image.schema.json")]
    // TODO: allof
    // TODO: oneOf
    public class Image
    {
        [JsonField(Name = "uri")]
        // TODO: "format": "uriref"
        public string Uri;

        [JsonField(Name = "mimeType")]
        // TODO: enum
        public string MimeType;

        [JsonField(Name = "bufferView")]
        [JsonSchema(Dependencies = new string[] { "mimeType" })]
        // TODO: all of
        public int bufferView;

        [JsonField(Name = "name")]
        public object name; // TODO: ignorable

        [JsonField(Name = "extensions")]
        public object Extensions; // TODO: ignorable

        [JsonField(Name = "extras")]
        public object Extras; // TODO: ignorable
    }
}
