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
using VGltf.Types.Extensions;

namespace VGltf.Unity
{
    public abstract class ExporterHookBase
    {
        public virtual void PostHook(Exporter exporter, Transform trans)
        {
        }
    }

    public sealed class Exporter : ExporterRefHookable<ExporterHookBase>, IDisposable
    {
        public class Config
        {
            public bool IncludeRootObject = true;
            public bool UseNormalizedTransforms = true;
        }

        sealed class InnerContext : IExporterContext
        {
            public Types.Gltf Gltf { get; }
            public ExporterRuntimeResources RuntimeResources { get; }
            public BufferBuilder BufferBuilder { get; }

            public NodeExporter Nodes { get; }
            public MeshExporter Meshes { get; }
            public MaterialExporter Materials { get; }
            public TextureExporter Textures { get; }
            public ImageExporter Images { get; }

            public InnerContext()
            {
                Gltf = new Types.Gltf();
                RuntimeResources = new ExporterRuntimeResources();
                BufferBuilder = new BufferBuilder();

                Nodes = new NodeExporter(this);
                Meshes = new MeshExporter(this);
                Materials = new MaterialExporter(this);
                Textures = new TextureExporter(this);
                Images = new ImageExporter(this);
            }

            void IDisposable.Dispose()
            {
                // TODO: Remove resources
            }
        }

        IExporterContext context_;
        public override IExporterContext Context { get => context_; }

        public Exporter()
        {
            context_ = new InnerContext();

            // Asset
            context_.Gltf.Asset = new Types.Asset
            {
                Version = "2.0", // glTF 2.0
                Generator = "VGltf"
            };
        }

        public void ExportGameObjectAsScene(GameObject go, Config config = null)
        {
            if (config == null)
            {
                config = new Config();
            }

            if (config.UseNormalizedTransforms)
            {
                using (var normalizer = new VGltf.Unity.Ext.TransformNormalizer())
                {
                    normalizer.Normalize(go);
                    ExportGameObjectAsSceneWithoutNormalize(normalizer.Go, config);
                }
            }
            else
            {
                ExportGameObjectAsSceneWithoutNormalize(go, config);
            }
        }

        void ExportGameObjectAsSceneWithoutNormalize(GameObject go, Config config)
        {
            Func<IndexedResource<Transform>[]> generator = () =>
            {
                if (config.IncludeRootObject)
                {
                    var node = Context.Nodes.Export(go);
                    return new IndexedResource<Transform>[] { node };
                }
                else
                {
                    return Enumerable.Range(0, go.transform.childCount).Select(i =>
                    {
                        var childGo = go.transform.GetChild(i);
                        return Context.Nodes.Export(childGo);
                    }).ToArray();
                }
            };
            var nodes = generator();

            // Scene
            var rootSceneIndex = Context.Gltf.AddScene(new Types.Scene
            {
                Nodes = nodes.Select(n => n.Index).ToArray(),
            });
            Context.Gltf.Scene = rootSceneIndex;

            foreach (var hook in Hooks)
            {
                hook.PostHook(this, go.transform);
            }
        }

        public GltfContainer IntoGlbContainer()
        {
            // Buffer
            List<Types.BufferView> views;
            var bufferBytes = Context.BufferBuilder.BuildBytes(out views);
            if (bufferBytes.Length > 0)
            {
                Context.Gltf.BufferViews = views;
                Context.Gltf.Buffers = new List<Types.Buffer> {
                    new Types.Buffer {
                        ByteLength = bufferBytes.Length,
                    }
                }; // Index0: references Glb bytes
            }

            var container = new GltfContainer(
                Context.Gltf,
                Context.Gltf.Buffers != null ? new Glb.StoredBuffer
                {
                    Payload = new ArraySegment<byte>(bufferBytes),
                } : null);
            return container;
        }

        // Take ownership of Context from exporter.
        public IExporterContext TakeContext()
        {
            var ctx = context_;
            context_ = null;

            return ctx;
        }

        void IDisposable.Dispose()
        {
            Context?.Dispose();
        }
    }
}
