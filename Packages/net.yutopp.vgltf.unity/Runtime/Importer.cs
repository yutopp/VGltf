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
    public abstract class ImporterHookBase
    {
        public virtual void PostHook(Importer importer)
        {
        }
    }

    public sealed class Importer : ImporterRefHookable<ImporterHookBase>, IDisposable
    {
        sealed class InnerContext : IImporterContext
        {
            public GltfContainer Container { get; }
            public ResourcesStore GltfResources { get; }

            public ImporterRuntimeResources Resources { get; }

            public ResourceImporters Importers { get; }

            public NodeImporter Nodes { get; }
            public MeshImporter Meshes { get; }
            public MaterialImporter Materials { get; }
            public TextureImporter Textures { get; }
            public ImageImporter Images { get; }

            public InnerContext(GltfContainer container, IResourceLoader loader)
            {
                Container = container;
                GltfResources = new ResourcesStore(container.Gltf, container.Buffer, loader);

                Resources = new ImporterRuntimeResources();

                Importers = new ResourceImporters
                {
                    Nodes = new NodeImporter(this),
                    Meshes = new MeshImporter(this),
                    Materials = new MaterialImporter(this),
                    Textures = new TextureImporter(this),
                    Images = new ImageImporter(this),
                };
            }

            void IDisposable.Dispose()
            {
                // TODO: implement
            }
        }

        IImporterContext context_;

        public override IImporterContext Context { get => context_; }

        public Importer(GltfContainer container, IResourceLoader loader)
        {
            context_ = new InnerContext(container, loader);
        }

        public Importer(GltfContainer container)
            : this(container, new ResourceLoaderFromEmbedOnly())
        {
        }

        public IImporterContext ImportSceneNodes()
        {
            var gltf = Context.Container.Gltf;
            var gltfScene = VGltf.Types.Extensions.GltfExtensions.GetSceneObject(gltf);

            var nodesCache = new NodesCache();
            foreach (var nodeIndex in gltfScene.Nodes)
            {
                Context.Importers.Nodes.ImportGameObjects(nodeIndex, nodesCache);
            }
            foreach (var nodeIndex in gltfScene.Nodes)
            {
                Context.Importers.Nodes.ImportMeshesAndSkins(nodeIndex, nodesCache);
            }

            foreach (var hook in Hooks)
            {
                hook.PostHook(this);
            }

            return TakeContext();
        }

        // Take ownership of Context from importer.
        public IImporterContext TakeContext()
        {
            var ctx = context_;
            context_ = null;

            return ctx;
        }

        void IDisposable.Dispose()
        {
            context_?.Dispose();
            context_ = null;
        }
    }
}
