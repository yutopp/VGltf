//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Collections.Generic;
using VGltf.Types;

namespace VGltf
{
    public class BufferBuilder
    {
        class AsView
        {
            public ArraySegment<byte> Payload;
            public int? ByteStride;
            public BufferView.TargetEnum? Target;
        }

        private List<AsView> _asViews = new List<AsView>();

        // TODO: Support alignments
        public int AddView(ArraySegment<byte> payload, int? byteStride = null, BufferView.TargetEnum? target = null)
        {
            var n = _asViews.Count;
            _asViews.Add(new AsView
            {
                Payload = payload,
                ByteStride = byteStride,
                Target = target,
            });
            return n;
        }

        public byte[] BuildBytes(out List<BufferView> views)
        {
            views = new List<BufferView>();
            using (var ms = new MemoryStream())
            {
                foreach (var asView in _asViews)
                {
                    var offset = ms.Length;

                    ms.Write(asView.Payload.Array, asView.Payload.Offset, asView.Payload.Count);

                    views.Add(new BufferView
                    {
                        Buffer = 0, // NOTE: Hardcoded
                        ByteOffset = (int)offset,
                        ByteLength = asView.Payload.Count,
                        ByteStride = asView.ByteStride,
                        Target = asView.Target,
                    });
                }

                return ms.ToArray();
            }
        }
    }
}
