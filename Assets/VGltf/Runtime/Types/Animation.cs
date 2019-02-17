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
    [JsonSchema(Id = "animation.schema.json")]
    public class Animation
    {
        [JsonField(Name = "channels")]
        [JsonSchema(MinItems = 1)]
        [JsonSchemaRequired]
        public List<ChannelType> Channels;

        [JsonField(Name = "samplers")]
        [JsonSchema(MinItems = 1)]
        [JsonSchemaRequired]
        public List<SamplerType> Samplers;

        [JsonField(Name = "name")]
        public object name; // TODO: ignorable

        [JsonField(Name = "extensions")]
        public object Extensions; // TODO: ignorable

        [JsonField(Name = "extras")]
        public object Extras; // TODO: ignorable

        //

        [JsonSchema(Id = "animation.channel.schema.json")]
        public class ChannelType
        {
            // TODO
        }

        [JsonSchema(Id = "animation.sampler.schema.json")]
        public class SamplerType
        {
            // TODO
        }
    }
}
