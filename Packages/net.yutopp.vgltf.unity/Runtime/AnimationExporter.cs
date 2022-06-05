//
// Copyright (c) 2022- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using UnityEngine;

namespace VGltf.Unity
{
    public abstract class AnimationExporterHook
    {
        public abstract IndexedResource<AnimationClip> Export(IExporterContext context, AnimationClip clip);
    }

    public sealed class AnimationExporter : ExporterRefHookable<AnimationExporterHook>
    {
        public override IExporterContext Context { get; }

        public AnimationExporter(IExporterContext context)
        {
            Context = context;
        }

        public IndexedResource<AnimationClip> Export(AnimationClip clip)
        {
            return Context.Resources.Animations.GetOrCall(clip, () =>
            {
                return ForceExport(clip);
            });
        }

        public IndexedResource<AnimationClip> ForceExport(AnimationClip clip)
        {
            foreach (var h in Hooks)
            {
                var r = h.Export(Context, clip);
                if (r != null)
                {
                    return r;
                }
            }

            throw new NotImplementedException();
        }
    }
}
