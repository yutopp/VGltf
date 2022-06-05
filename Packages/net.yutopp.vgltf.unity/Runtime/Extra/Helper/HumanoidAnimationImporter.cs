//
// Copyright (c) 2022- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VGltf.Unity.Ext.Helper
{
    public sealed class HumanoidAnimationImporter : AnimationImporterHook
    {
        public override Task<IndexedResource<AnimationClip>> Import(IImporterContext context, int animIndex, CancellationToken ct)
        {
            var gltf = context.Container.Gltf;
            var gltfAnim = gltf.Animations[animIndex];

            var animExtra = default(Types.HumanoidAnimationType);
            if (!gltfAnim.TryGetExtra(Types.HumanoidAnimationType.ExtraName, context.Container.JsonSchemas, out animExtra))
            {
                // This importer aims to importing only HumanoidAnimation extras, thus skip process.
                return null;
            }

            var animClip = new AnimationClip();
            animClip.name = gltfAnim.Name;

            var resource = context.Resources.Animations.Add(animIndex, animIndex, animClip.name, animClip);

            foreach (var channel in gltfAnim.Channels)
            {
                var extra = default(Types.HumanoidAnimationType.ChannelType);
                if (!channel.TryGetExtra(Types.HumanoidAnimationType.ChannelType.ExtraName, context.Container.JsonSchemas, out extra))
                {
                    throw new NotImplementedException();
                }

                var sampler = gltfAnim.Samplers[channel.Sampler];

                var timestamps = ImportTimestamp(context, sampler.Input);
                var values = ImportHumanoidValue(context, sampler.Output);
                Debug.Assert(timestamps.Length == values.Length);

                // TODO: support interpolation
                var keyframes = new Keyframe[timestamps.Length];
                for (var i = 0; i < timestamps.Length; ++i)
                {
                    keyframes[i] = new Keyframe(timestamps[i], values[i]);
                }

                var curve = new AnimationCurve(keyframes);
                animClip.SetCurve("", typeof(Animator), extra.PropertyName, curve);
            }

            return Task.FromResult(resource);
        }

        public static float[] ImportTimestamp(IImporterContext context, int index)
        {
            // https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#_animation_sampler_input

            // SCALAR | FLOAT
            var buf = context.GltfResources.GetOrLoadTypedBufferByAccessorIndex(index);
            var acc = buf.Accessor;
            if (acc.Type == VGltf.Types.Accessor.TypeEnum.Scalar)
            {
                if (acc.ComponentType == VGltf.Types.Accessor.ComponentTypeEnum.FLOAT)
                {
                    return buf.GetEntity<float, float>((xs, i) => xs[i]).AsArray();
                }
            }

            throw new NotImplementedException(); // TODO
        }

        public static float[] ImportHumanoidValue(IImporterContext context, int index)
        {
            // https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#_animation_sampler_input

            // SCALAR | FLOAT
            var buf = context.GltfResources.GetOrLoadTypedBufferByAccessorIndex(index);
            var acc = buf.Accessor;
            if (acc.Type == VGltf.Types.Accessor.TypeEnum.Scalar)
            {
                if (acc.ComponentType == VGltf.Types.Accessor.ComponentTypeEnum.FLOAT)
                {
                    return buf.GetEntity<float, float>((xs, i) => xs[i]).AsArray();
                }
            }

            throw new NotImplementedException(); // TODO
        }
    }
}
