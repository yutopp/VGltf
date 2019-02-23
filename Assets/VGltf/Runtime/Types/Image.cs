//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using VJson;
using VJson.Schema;

// Reference: https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/*
namespace VGltf.Types
{
    [JsonSchema(Id = "image.schema.json")]
    // TODO: allof
    // TODO: oneOf
    public class Image : GltfChildOfRootProperty
    {
        [JsonField(Name = "uri")]
        // TODO: "format": "uriref"
        public string Uri;

        [JsonField(Name = "mimeType")]
        public MimeTypeEnum MimeType;

        [JsonField(Name = "bufferView")]
        [JsonSchemaDependencies("mimeType")]
        // TODO: all of
        public int bufferView;

        //

        [Json(EnumConversion = EnumConversionType.AsString)]
        public enum MimeTypeEnum
        {
            [JsonField(Name = "image/jpeg")]
            ImageJpeg,
            [JsonField(Name = "image/png")]
            ImagePng,
        }
    }
}
