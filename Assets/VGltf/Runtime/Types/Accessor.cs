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

// Reference: https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/*
namespace VGltf.Types
{
    [JsonSchema(Id = "accessor.schema.json")]
    // TODO: support all of
    public class Accessor : GltfChildOfRootProperty
    {
        // TODO: allOf": [ { "$ref": "glTFid.schema.json"} ]
        [JsonField(Name = "BufferView"), JsonFieldIgnorable] // TODO: fix default value. Use optional?
        public int BufferView; // TODO: ignorable

        [JsonField(Name = "byteOffset")]
        [JsonSchema(Minimum = 0), JsonSchemaDependencies("bufferView")]
        public int ByteOffset = 0;

        [JsonField(Name = "componentType")]
        [JsonSchemaRequired]
        public ComponentTypeEnum ComponentType; // TODO enum

        [JsonField(Name = "normalized")]
        public bool Normalized = false;

        [JsonField(Name = "count")]
        [JsonSchema(Minimum = 1), JsonSchemaRequired]
        public int Count;

        [JsonField(Name = "type")]
        [JsonSchemaRequired]
        public TypeEnum Type;

        [JsonField(Name = "max")]
        [JsonSchema(MinItems = 1, MaxItems = 16)]
        public float[] Max; // TODO: ignorable

        [JsonField(Name = "min")]
        [JsonSchema(MinItems = 1, MaxItems = 16)]
        public float[] Min; // TODO: ignorable

        [JsonField(Name = "sparse")]
        public SparseType Sparse; // TODO: ignorable

        //

        public enum ComponentTypeEnum
        {
            BYTE = 5120,
            UNSIGNED_BYTE = 5121,
            SHORT = 5122,
            UNSIGNED_SHORT = 5123,
            UNSIGNED_INT = 5125,
            FLOAT = 5126,
        }

        [Json(EnumConversion = EnumConversionType.AsString)]
        public enum TypeEnum
        {
            [JsonField(Name = "SCALAR")]
            Scalar,
            [JsonField(Name = "VEC2")]
            Vec2,
            [JsonField(Name = "VEC3")]
            Vec3,
            [JsonField(Name = "VEC4")]
            Vec4,
            [JsonField(Name = "MAT2")]
            Mat2,
            [JsonField(Name = "MAT3")]
            Mat3,
            [JsonField(Name = "MAT4")]
            Mat4
        }

        [JsonSchema(Id = "accessor.sparse.schema.json")]
        public class SparseType : GltfProperty
        {
            [JsonField(Name = "count")]
            [JsonSchema(Minimum = 0), JsonSchemaRequired]
            public int Count;

            [JsonField(Name = "indices")]
            [JsonSchemaRequired]
            public IndicesType Indices;

            [JsonField(Name = "values")]
            [JsonSchemaRequired]
            public ValuesType Values;

            //

            [JsonSchema(Id = "accessor.sparse.indices.schema.json")]
            public class IndicesType : GltfProperty
            {
                [JsonField(Name = "bufferView")]
                [JsonSchemaRequired]
                // TODO: "$ref": "glTFid.schema.json"
                public int BufferView;

                [JsonField(Name = "byteOffset")]
                [JsonSchema(Minimum = 0)]
                public int ByteOffset = 0;

                [JsonField(Name = "componentType")]
                [JsonSchemaRequired]
                public ComponentTypeEnum ComponentType;

                //

                public enum ComponentTypeEnum
                {
                    UNSIGNED_BYTE = 5121,
                    UNSIGNED_SHORT = 5123,
                    UNSIGNED_INT = 5125,
                }
            }

            [JsonSchema(Id = "accessor.sparse.values.schema.json")]
            public class ValuesType : GltfProperty
            {
                [JsonField(Name = "bufferView")]
                [JsonSchemaRequired]
                // TODO: "$ref": "glTFid.schema.json"
                public int BufferView;

                [JsonField(Name = "byteOffset")]
                [JsonSchema(Minimum = 0)]
                public int ByteOffset = 0;
            }
        }
    }
}
