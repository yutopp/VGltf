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
    [JsonSchema(Id = "camera.schema.json")]
    // TODO: allof
    // TODO: not
    public class Camera : GltfChildOfRootProperty
    {
        [JsonField(Name = "orthographic")]
        public OrthographicType Orthographic; // TODO: ignorable

        [JsonField(Name = "perspective")]
        public PerspectiveType Perspective; // TODO: ignorable

        [JsonField(Name = "type")]
        [JsonSchemaRequired]
        public TypeEnum Type;

        //

        [JsonSchema(Id = "camera.orthographic.schema.json")]
        public class OrthographicType : GltfProperty
        {
            [JsonField(Name = "xmag")]
            [JsonSchemaRequired]
            public float Xmag; // TODO: ignorable

            [JsonField(Name = "ymag")]
            [JsonSchemaRequired]
            public float Ymag; // TODO: ignorable

            [JsonField(Name = "zfar")]
            [JsonSchema(ExclusiveMinimum = 0.0f), JsonSchemaRequired]
            public float Zfar; // TODO: ignorable

            [JsonField(Name = "znear")]
            [JsonSchema(Minimum = 0.0f), JsonSchemaRequired]
            public float Znear; // TODO: ignorable
        }

        [JsonSchema(Id = "camera.perspective.schema.json")]
        public class PerspectiveType
        {
            [JsonField(Name = "aspectRatio")]
            [JsonSchema(ExclusiveMinimum = 0.0f)]
            public float AspectRatio; // TODO: ignorable

            [JsonField(Name = "yfov")]
            [JsonSchema(ExclusiveMinimum = 0.0f), JsonSchemaRequired]
            public float Yfov; // TODO: ignorable

            [JsonField(Name = "zfar")]
            [JsonSchema(ExclusiveMinimum = 0.0f)]
            public float Zfar; // TODO: ignorable

            [JsonField(Name = "znear")]
            [JsonSchema(ExclusiveMinimum = 0.0f), JsonSchemaRequired]
            public float Znear; // TODO: ignorable
        }

        [Json(EnumConversion = EnumConversionType.AsString)]
        public enum TypeEnum
        {
            [JsonField(Name = "perspective")]
            Perspective,
            [JsonField(Name = "orthographic")]
            Orthographic,
        }
    }
}
