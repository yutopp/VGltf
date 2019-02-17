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
    [JsonSchema(Id = "skin.schema.json")]
    public class Skin
    {
        [JsonField(Name = "inverseBindMatrices")]
        // TODO
        public int InverseBindMatrices;

        [JsonField(Name = "skeleton")]
        // TODO
        public int Skeleton;

        [JsonField(Name = "joints")]
        [JsonSchema(/*uniqueItems*/MinItems = 1)]
        [JsonSchemaRequired]
        public int[] Joints;

        [JsonField(Name = "name")]
        public object Name; // TODO: ignorable

        [JsonField(Name = "extensions")]
        public object Extensions; // TODO: ignorable

        [JsonField(Name = "extras")]
        public object Extras; // TODO: ignorable
    }
}
