//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;

namespace VGltf.Unity
{
    public interface IImporter
    {
        GltfContainer Container { get; }
        ResourcesCache<int> Cache { get; }
        ResourcesStore BufferView { get; }

        NodeImporter Nodes { get; }
        MeshImporter Meshes { get; }
        MaterialImporter Materials { get; }
        TextureImporter Textures { get; }
        ImageImporter Images { get; }
    }

    public abstract class ImporterRef : IImporter
    {
        private IImporter _importer;

        public GltfContainer Container { get => _importer.Container; }
        public ResourcesCache<int> Cache { get => _importer.Cache; }
        public ResourcesStore BufferView { get => _importer.BufferView; }

        public NodeImporter Nodes { get => _importer.Nodes; }
        public MeshImporter Meshes { get => _importer.Meshes; }
        public MaterialImporter Materials { get => _importer.Materials; }
        public TextureImporter Textures { get => _importer.Textures; }
        public ImageImporter Images { get => _importer.Images; }

        public ImporterRef(IImporter p)
        {
            _importer = p;
        }
    }

    public abstract class ImporterRefHookable<T> : ImporterRef
    {
        protected List<T> Hooks = new List<T>();

        public ImporterRefHookable(IImporter parent)
            : base(parent)
        {
        }

        public void AddHook(T hook)
        {
            Hooks.Add(hook);
        }
    }
}
