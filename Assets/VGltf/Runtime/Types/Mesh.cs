//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using VJson;
using VJson.Schema;

// Reference: https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/*
namespace VGltf.Types
{
    [JsonSchema(Id = "mesh.schema.json")]
    public class Mesh : GltfChildOfRootProperty
    {
        [JsonField(Name = "primitives")]
        [JsonSchema(MinItems = 1), JsonSchemaRequired]
        public List<PrimitiveType> Primitives;

        [JsonField(Name = "weights"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public float[] weights;

        //

        [JsonSchema(Id = "mesh.primitive.schema.json")]
        public class PrimitiveType : GltfProperty
        {
            [JsonField(Name = "attributes")]
            [JsonSchema(MinProperties = 1), JsonSchemaRequired]
            public Dictionary<string, uint> Attributes; // glTFid

            [JsonField(Name = "indices"), JsonFieldIgnorable]
            [JsonSchemaRef(typeof(GltfID))]
            public int? Indices;

            [JsonField(Name = "material"), JsonFieldIgnorable]
            [JsonSchemaRef(typeof(GltfID))]
            public int? Material;

            [JsonField(Name = "mode"), JsonFieldIgnorable]
            public ModeEnum? Mode = ModeEnum.TRIANGLES;

            [JsonField(Name = "targets"), JsonFieldIgnorable]
            [JsonSchema(MinItems = 1)]
            [ItemsJsonSchema(MinProperties = 1)]
            public List<Dictionary<string, uint>> Targets; // glTFid

            //

            public enum ModeEnum
            {
                POINTS = 0,
                LINES = 1,
                LINE_LOOP = 2,
                LINE_STRIP = 3,
                TRIANGLES = 4,
                TRIANGLE_STRIP = 5,
                TRIANGLE_FAN = 6,
            }
        }
    }
}
