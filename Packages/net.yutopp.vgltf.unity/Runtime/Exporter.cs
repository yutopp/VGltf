//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
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
            public bool FlipZAxisInsteadOfXAsix = false;
        }

        sealed class InnerContext : IExporterContext
        {
            public Types.Gltf Gltf { get; }
            public BufferBuilder BufferBuilder { get; }

            public ExporterRuntimeResources Resources { get; }

            public ResourceExporters Exporters { get; }

            public InnerContext(CoordUtils coordUtils)
            {
                Gltf = new Types.Gltf();
                BufferBuilder = new BufferBuilder();

                Resources = new ExporterRuntimeResources();

                Exporters = new ResourceExporters
                {
                    Nodes = new NodeExporter(this, coordUtils),
                    Meshes = new MeshExporter(this, coordUtils),
                    Materials = new MaterialExporter(this),
                    Textures = new TextureExporter(this),
                    Images = new ImageExporter(this),
                };
            }

            void IDisposable.Dispose()
            {
                // TODO: Remove resources
            }
        }

        readonly Config _config;

        IExporterContext context_;

        public override IExporterContext Context { get => context_; }

        public Exporter(Config config = null)
        {
            if (config == null)
            {
                config = new Config();
            }
            _config = config;

            var coordUtils = config.FlipZAxisInsteadOfXAsix ? new CoordUtils(new Vector3(1, 1, -1)) : new CoordUtils();
            context_ = new InnerContext(coordUtils);

            // Asset
            context_.Gltf.Asset = new Types.Asset
            {
                Version = "2.0", // glTF 2.0
                Generator = "VGltf"
            };
        }

        public void ExportGameObjectAsScene(GameObject go)
        {
            if (_config.UseNormalizedTransforms)
            {
                using (var normalizer = new VGltf.Unity.Ext.TransformNormalizer())
                {
                    normalizer.Normalize(go);
                    ExportGameObjectAsSceneWithoutNormalize(normalizer.Go);
                }
            }
            else
            {
                ExportGameObjectAsSceneWithoutNormalize(go);
            }
        }

        void ExportGameObjectAsSceneWithoutNormalize(GameObject go)
        {
            Func<IndexedResource<Transform>[]> generator = () =>
            {
                if (_config.IncludeRootObject)
                {
                    var node = Context.Exporters.Nodes.Export(go);
                    return new IndexedResource<Transform>[] { node };
                }
                else
                {
                    return Enumerable.Range(0, go.transform.childCount).Select(i =>
                    {
                        var childGo = go.transform.GetChild(i);
                        return Context.Exporters.Nodes.Export(childGo);
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
                    // Buffers[0]: references Glb bytes
                    new Types.Buffer {
                        ByteLength = bufferBytes.Length,
                    }
                };
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
            context_ = null;
        }
    }
}
