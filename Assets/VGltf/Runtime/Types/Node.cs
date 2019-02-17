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
    [JsonSchema(Id = "node.schema.json")]
    // TODO: not
    public class Node
    {
        [JsonField(Name = "camera")]
        // TODO
        public int Camera;

        [JsonField(Name = "children")]
        [JsonSchema(/*UniqueItems=true, */MinItems=1)]
        // TODO
        public int[] Children;

        [JsonField(Name = "skin")]
        // TODO
        [JsonSchemaDependencies("mesh")]
        public int Skin;

        [JsonField(Name = "matrix")]
        [JsonSchema(MinItems=16, MaxItems=16)]
        public float[] Matrix = new float[]{ 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f};

        [JsonField(Name = "mesh")]
        // TODO
        public int Mesh;

        [JsonField(Name = "rotation")]
        [JsonSchema(MinItems=4, MaxItems=4)]
        [ItemsJsonSchema(Minimum=-1.0, Maximum=1.0)]
        public float[] Rotation = new float[] { 0.0f, 0.0f, 0.0f, 1.0f};

        [JsonField(Name = "scale")]
        [JsonSchema(MinItems=3, MaxItems=3)]
        public float[] Scale = new float[] { 1.0f, 1.0f, 1.0f};

        [JsonField(Name = "translation")]
        [JsonSchema(MinItems=3, MaxItems=3)]
        public float[] Translation = new float[] { 0.0f, 0.0f, 0.0f};

        [JsonField(Name = "weights")]
        [JsonSchema(MinItems=1)]
        [JsonSchemaDependencies("mesh")]
        public float[] Weights;

        [JsonField(Name = "name")]
        public object name; // TODO: ignorable

        [JsonField(Name = "extensions")]
        public object Extensions; // TODO: ignorable

        [JsonField(Name = "extras")]
        public object Extras; // TODO: ignorable
    }
}
