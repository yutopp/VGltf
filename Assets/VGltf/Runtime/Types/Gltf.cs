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
    [JsonSchema(Id = "glTF.schema.json")]
    public class Gltf : GltfProperty
    {
        [JsonField(Name = "extensionsUsed"), JsonFieldIgnorable]
        [JsonSchema(UniqueItems = true, MinItems = 1)]
        public string[] ExtensionsUsed;

        [JsonField(Name = "extensionsRequired"), JsonFieldIgnorable]
        [JsonSchema(UniqueItems = true, MinItems = 1)]
        public string[] ExtensionsRequired;

        [JsonField(Name = "accessors"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Accessor> Accessors;

        [JsonField(Name = "animations"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Animation> Animations;

        [JsonField(Name = "asset")]
        [JsonSchemaRequired]
        public Asset Asset;

        [JsonField(Name = "buffers"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Buffer> Buffers;

        [JsonField(Name = "bufferViews"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<BufferView> BufferViews;

        [JsonField(Name = "cameras"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Camera> Cameras;

        [JsonField(Name = "images"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Image> Images;

        [JsonField(Name = "materials"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Material> Materials;

        [JsonField(Name = "meshes"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Mesh> Meshes;

        [JsonField(Name = "nodes"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Node> Nodes;

        [JsonField(Name = "samplers"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Sampler> Samplers;

        [JsonField(Name = "scene"), JsonFieldIgnorable]
        [JsonSchemaDependencies("scenes")]
        // TODO: allOf": [ { "$ref": "glTFid.schema.json"} ]
        public int Scene;

        [JsonField(Name = "scenes"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Scene> Scenes;

        [JsonField(Name = "skins"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Skin> Skins;

        [JsonField(Name = "textures"), JsonFieldIgnorable]
        [JsonSchema(MinItems = 1)]
        public List<Texture> Textures;
    }
}
