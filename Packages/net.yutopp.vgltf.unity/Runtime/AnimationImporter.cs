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

namespace VGltf.Unity
{
    public abstract class AnimationImporterHook
    {
        public abstract Task<IndexedResource<AnimationClip>> Import(IImporterContext context, int animIndex, CancellationToken ct);
    }

    public sealed class AnimationImporter : ImporterRefHookable<AnimationImporterHook>
    {
        public override IImporterContext Context { get; }

        public AnimationImporter(IImporterContext context)
        {
            Context = context;
        }

        public async Task<IndexedResource<AnimationClip>> Import(int animIndex, CancellationToken ct)
        {
            return await Context.Resources.Animations.GetOrCallAsync(animIndex, async () =>
            {
                return await ForceImport(animIndex, ct);
            });
        }

        public async Task<IndexedResource<AnimationClip>> ForceImport(int animIndex, CancellationToken ct)
        {
            foreach (var h in Hooks)
            {
                var r = await h.Import(Context, animIndex, ct);
                if (r != null)
                {
                    return r;
                }
            }

            throw new NotImplementedException();
        }
    }
}
