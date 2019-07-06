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
    public class Exporter : IExporter, IDisposable
    {
        public Types.Gltf Gltf { get; }
        public ResourcesCache<string> Cache { get; }
        public BufferBuilder BufferBuilder { get; }

        public NodeExporter Nodes { get; }
        public MeshExporter Meshes { get; }
        public MaterialExporter Materials { get; }
        public TextureExporter Textures { get; }
        public ImageExporter Images { get; }

        public Exporter()
        {
            Gltf = new Types.Gltf();
            Cache = new ResourcesCache<string>();
            BufferBuilder = new BufferBuilder();

            Nodes = new NodeExporter(this);
            Meshes = new MeshExporter(this);
            Materials = new MaterialExporter(this);
            Textures = new TextureExporter(this);
            Images = new ImageExporter(this);

            // Asset
            Gltf.Asset = new Types.Asset
            {
                Version = "2.0", // glTF 2.0
            };
        }

        public void ExportGameObjectAsScene(GameObject go)
        {
            var nodeExporter = new NodeExporter(this);
            var rootNodeResource = nodeExporter.Export(go);

            // Scene
            var rootSceneIndex = Types.GltfExtensions.AddScene(Gltf, new Types.Scene
            {
                Nodes = new int[] { rootNodeResource.Index },
            });
            Gltf.Scene = rootSceneIndex;
        }

        public GltfContainer IntoGlbContainer()
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
