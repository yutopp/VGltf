//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace VGltf.Unity
{
    public class Exporter : ExporterBase, IDisposable
    {
        public Exporter(GameObject go)
            : base(new Types.Gltf(), new ResourcesCache(), new BufferBuilder())
        {
            var nodeExporter = new NodeExporter(this);
            var rootNodeIndex = nodeExporter.Export(go);

            // Asset
            Gltf.Asset = new Types.Asset
            {
                Version = "2.0", // glTF 2.0
            };

            // Scene
            if (rootNodeIndex != null)
            {
                var rootSceneIndex = Types.GltfExtensions.AddScene(Gltf, new Types.Scene
                {
                    Nodes = new int[] { rootNodeIndex.Value },
                });
                Gltf.Scene = rootSceneIndex;
            }
        }

        public GltfContainer Export()
        {
            // Buffer
            List<Types.BufferView> views;
            var bufferBytes = BufferBuilder.BuildBytes(out views);
            if (bufferBytes.Length > 0)
            {
                Gltf.BufferViews = views;
                Gltf.Buffers = new List<Types.Buffer> {
                    new Types.Buffer {
                        ByteLength = bufferBytes.Length,
                    }
                }; // Index0: references Glb bytes
            }

            var container = new GltfContainer(
                Gltf,
                Gltf.Buffers != null ? new Glb.StoredBuffer
                {
                    Payload = new ArraySegment<byte>(bufferBytes),
                } : null);
            return container;
        }

        public void Dispose()
        {
            // TODO: Remove resources
        }
    }
}
