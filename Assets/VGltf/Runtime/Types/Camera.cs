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
    public class Camera
    {
        [JsonField(Name = "orthographic")]
        // TODO: allof
        public OrthographicType Orthographic; // TODO: ignorable

        [JsonField(Name = "perspective")]
        // TODO: allof
        public PerspectiveType Perspective; // TODO: ignorable

        [JsonField(Name = "type")]
        [JsonSchemaRequired]
        // TODO: enum
        public string Type;

        [JsonField(Name = "name")]
        public object name; // TODO: ignorable

        [JsonField(Name = "extensions")]
        public object Extensions; // TODO: ignorable

        [JsonField(Name = "extras")]
        public object Extras; // TODO: ignorable

        //

        [JsonSchema(Id="camera.orthographic.schema.json")]
        public class OrthographicType
        {
            // TODO
        }

        [JsonSchema(Id="camera.perspective.schema.json")]
        public class PerspectiveType
        {
            // TODO
        }
    }
}
