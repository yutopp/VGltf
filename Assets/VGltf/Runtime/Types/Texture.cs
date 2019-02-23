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
    [JsonSchema(Id = "texture.schema.json")]
    public class Texture : GltfChildOfRootProperty
    {
        [JsonField(Name = "sampler")]
        // TODO: "$ref": "glTFid.schema.json"
        public int Sampler;

        [JsonField(Name = "source")]
        // TODO: "$ref": "glTFid.schema.json"
        public int Source;
    }

    public class TextureInfo : GltfProperty
    {
        [JsonField(Name = "index")]
        [JsonSchemaRequired]
        // TODO: "$ref": "glTFid.schema.json"
        public int Index;

        [JsonField(Name = "texCoord")]
        [JsonSchema(Minimum = 0), JsonSchemaRequired]
        public int TexCoord = 0;
    }
}
