//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
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
    public abstract class ImporterHookBase
    {
        public virtual async Task PostHook(IImporterContext context, CancellationToken ct)
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
            public ITimeSlicer TimeSlicer { get; }

            public ResourceImporters Importers { get; }

            public InnerContext(GltfContainer container, IResourceLoader loader, ITimeSlicer timeSlicer, CoordUtils coordUtils)
            {
                Container = container;
                GltfResources = new ResourcesStore(container, loader);

                Resources = new ImporterRuntimeResources();
                TimeSlicer = timeSlicer;

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

        IImporterContext _context;

        public override IImporterContext Context { get => _context; }

        public Importer(GltfContainer container, IResourceLoader loader, ITimeSlicer timeSlicer, Config config = null)
        {
            if (config == null)
            {
                config = new Config();
            }

            var coordUtils = config.FlipZAxisInsteadOfXAsix ? new CoordUtils(new Vector3(1, 1, -1)) : new CoordUtils();
            _context = new InnerContext(container, loader, timeSlicer, coordUtils);
        }

        public Importer(GltfContainer container, ITimeSlicer timeSlicer, Config config = null)
            : this(container, new ResourceLoaderFromEmbedOnly(), timeSlicer, config)
        {
        }

        public async Task<IImporterContext> ImportSceneNodes(CancellationToken ct)
        {
            var gltf = Context.Container.Gltf;
            var gltfScene = VGltf.Types.Extensions.GltfExtensions.GetSceneObject(gltf);

            foreach (var nodeIndex in gltfScene.Nodes)
            {
                Context.Importers.Nodes.ImportGameObjects(nodeIndex);
            }
            foreach (var nodeIndex in gltfScene.Nodes)
            {
                await Context.Importers.Nodes.ImportMeshesAndSkins(nodeIndex, ct);
                await _context.TimeSlicer.Slice(ct);
            }

            // await Context.Tasks.Run();

            foreach (var hook in Hooks)
            {
                await hook.PostHook(Context, ct);
                await _context.TimeSlicer.Slice(ct);
            }

            return TakeContext();
        }

        // Take ownership of Context from importer.
        public IImporterContext TakeContext()
        {
            var ctx = _context;
            _context = null;

            return ctx;
        }

        void IDisposable.Dispose()
        {
            _context?.Dispose();
            _context = null;
        }
    }
}
