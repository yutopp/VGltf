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
    [JsonSchema(Title = "glTF",
                Description = "The root object for a glTF asset.",
                Id = "glTF.schema.json")]
    // TODO: support all of
    public class Gltf
    {
        [JsonField(Name = "extensionsUsed")]
        [JsonSchema(/*uniqueItems = true*/
                    MinItems = 1)]
        public string[] ExtensionsUsed; // TODO: ignorable

        [JsonField(Name = "extensionsRequired")]
        [JsonSchema(/*uniqueItems = true*/
                    MinItems = 1)]
        public string[] ExtensionsRequired; // TODO: ignorable

        [JsonField(Name = "accessors")]
        [JsonSchema(MinItems = 1)]
        public List<Accessor> Accessors; // TODO: ignorable

        [JsonField(Name = "animations")]
        [JsonSchema(MinItems = 1)]
        public List<Animation> Animations; // TODO: ignorable

        [JsonField(Name = "asset")]
        [JsonSchemaRequired]
        /* TODO: allOf */
        public Asset Asset;

        [JsonField(Name = "buffers")]
        [JsonSchema(MinItems = 1)]
        public List<Buffer> Buffers; // TODO: ignorable

        [JsonField(Name = "bufferViews")]
        [JsonSchema(MinItems = 1)]
        public List<BufferView> BufferViews; // TODO: ignorable

        [JsonField(Name = "cameras")]
        [JsonSchema(MinItems = 1)]
        public List<Camera> Cameras; // TODO: ignorable

        [JsonField(Name = "images")]
        [JsonSchema(MinItems = 1)]
        public List<Image> Images; // TODO: ignorable

        [JsonField(Name = "materials")]
        [JsonSchema(MinItems = 1)]
        public List<Material> Materials; // TODO: ignorable

        [JsonField(Name = "meshes")]
        [JsonSchema(MinItems = 1)]
        public List<Mesh> Meshes; // TODO: ignorable

        [JsonField(Name = "nodes")]
        [JsonSchema(MinItems = 1)]
        public List<Node> Nodes; // TODO: ignorable

        [JsonField(Name = "samplers")]
        [JsonSchema(MinItems = 1)]
        public List<Sampler> Samplers; // TODO: ignorable

        [JsonField(Name = "scene")]
        [JsonSchemaDependencies(new string[] { "scenes" })]
        // TODO: allOf": [ { "$ref": "glTFid.schema.json"} ]
        public int Scene; // TODO: ignorable

        [JsonField(Name = "scenes")]
        [JsonSchema(MinItems = 1)]
        public List<Scene> Scenes; // TODO: ignorable

        [JsonField(Name = "skins")]
        [JsonSchema(MinItems = 1)]
        public List<Skin> Skins; // TODO: ignorable

        [JsonField(Name = "textures")]
        [JsonSchema(MinItems = 1)]
        public List<Texture> Textures; // TODO: ignorable

        [JsonField(Name = "extensions")]
        public object Extensions; // TODO: ignorable

        [JsonField(Name = "extras")]
        public object Extras; // TODO: ignorable
    }
}
