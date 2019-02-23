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
    [JsonSchema(Id = "material.schema.json")]
    public class Material : GltfChildOfRootProperty
    {
        [JsonField(Name = "pbrMetallicRoughness")]
        public PbrMetallicRoughnessType PbrMetallicRoughness;

        [JsonField(Name = "normalTexture")]
        public NormalTextureInfoType NormalTexture;

        [JsonField(Name = "occlusionTexture")]
        public OcclusionTextureType OcclusionTexture;

        [JsonField(Name = "emissiveTexture")]
        public TextureInfo EmissiveTexture;

        [JsonField(Name = "emissiveFactor")]
        [JsonSchema(MinItems = 3, MaxItems = 3)]
        [ItemsJsonSchema(Minimum = 0.0, Maximum = 1.0)]
        public float[] EmissiveFactor = new float[] { 0.0f, 0.0f, 0.0f };

        [JsonField(Name = "alphaMode")]
        public AlphaModeEnum AlphaMode = AlphaModeEnum.Opaque;

        [JsonField(Name = "alphaCutoff")]
        [JsonSchema(Minimum = 0.0)]
        [JsonSchemaDependencies(new string[] { "alphaMode" })]
        public float AlphaCutoff = 0.5f;

        [JsonField(Name = "doubleSided")]
        public bool DoubleSided = false;

        //

        [JsonSchema(Id = "material.pbrMetallicRoughness.schema.json")]
        public class PbrMetallicRoughnessType
        {
        }

        [JsonSchema(Id = "material.normalTextureInfo.schema.json")]
        public class NormalTextureInfoType
        {
        }

        [JsonSchema(Id = "material.occlusionTexture.schema.json")]
        public class OcclusionTextureType
        {
        }

        [Json(EnumConversion = EnumConversionType.AsString)]
        public enum AlphaModeEnum
        {
            [JsonField(Name = "OPAQUE")]
            Opaque,
            [JsonField(Name = "MASK")]
            Mask,
            [JsonField(Name = "BLEND")]
            Blend,
        }
    }
}
