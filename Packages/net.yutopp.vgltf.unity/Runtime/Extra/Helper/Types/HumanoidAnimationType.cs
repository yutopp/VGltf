//
// Copyright (c) 2022- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using VJson;

namespace VGltf.Unity.Ext.Helper.Types
{
    /// <summary>
    /// </summary>
    [Json]
    public sealed class HumanoidAnimationType
    {
        public static readonly string ExtraName = "VGLTF_unity_humanoid_animation";

        [Json]
        public sealed class ChannelType
        {
            public static readonly string ExtraName = "VGLTF_unity_humanoid_animation_channel";

            [JsonField(Name = "propertyName")]
            public string PropertyName;
        }
    }
}
