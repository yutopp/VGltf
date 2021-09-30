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
        public sealed class Config
        {
            public bool FlipZAxisInsteadOfXAsix = false;
        }

        sealed class InnerContext : IImporterContext
        {
            public GltfContainer Container { get; }
            public ResourcesStore GltfResources { get; }

            public ImporterRuntimeResources Resources { get; }

            public ResourceImporters Importers { get; }

            public InnerContext(GltfContainer container, IResourceLoader loader, CoordUtils coordUtils)
            {
                Container = container;
                GltfResources = new ResourcesStore(container, loader);

                Resources = new ImporterRuntimeResources();

                Importers = new ResourceImporters
                {
                    Nodes = new NodeImporter(this, coordUtils),
                    Meshes = new MeshImporter(this, coordUtils),
                    Materials = new MaterialImporter(this),
                    Textures = new TextureImporter(this),
                    Images = new ImageImporter(this),
                };
            }

            void IDisposable.Dispose()
            {
                Resources.Dispose();
            }
        }

        IImporterContext context_;

        public override IImporterContext Context { get => context_; }

        public Importer(GltfContainer container, IResourceLoader loader, Config config = null)
        {
            if (config == null)
            {
                config = new Config();
            }

            var coordUtils = config.FlipZAxisInsteadOfXAsix ? new CoordUtils(new Vector3(1, 1, -1)) : new CoordUtils();
            context_ = new InnerContext(container, loader, coordUtils);
        }

        public Importer(GltfContainer container, Config config = null)
            : this(container, new ResourceLoaderFromEmbedOnly(), config)
        {
        }

        public IImporterContext ImportSceneNodes()
        {
            var gltf = Context.Container.Gltf;
            var gltfScene = VGltf.Types.Extensions.GltfExtensions.GetSceneObject(gltf);

            foreach (var nodeIndex in gltfScene.Nodes)
            {
                Context.Importers.Nodes.ImportGameObjects(nodeIndex);
            }
            foreach (var nodeIndex in gltfScene.Nodes)
            {
                Context.Importers.Nodes.ImportMeshesAndSkins(nodeIndex);
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
