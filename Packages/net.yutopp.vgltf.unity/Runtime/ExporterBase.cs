//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;

namespace VGltf.Unity
{
    public interface IExporter
    {
        Types.Gltf Gltf { get; }
        ResourcesCache<string> Cache { get; }
        BufferBuilder BufferBuilder { get; }

        NodeExporter Nodes { get; }
        MeshExporter Meshes { get; }
        MaterialExporter Materials { get; }
        TextureExporter Textures { get; }
        ImageExporter Images { get; }
    }

    public abstract class ExporterRef : IExporter
    {
        private IExporter _exporter;

        public Types.Gltf Gltf { get => _exporter.Gltf; }
        public ResourcesCache<string> Cache { get => _exporter.Cache; }
        public BufferBuilder BufferBuilder { get => _exporter.BufferBuilder; }

        public NodeExporter Nodes { get => _exporter.Nodes; }
        public MeshExporter Meshes { get => _exporter.Meshes; }
        public MaterialExporter Materials { get => _exporter.Materials; }
        public TextureExporter Textures { get => _exporter.Textures; }
        public ImageExporter Images { get => _exporter.Images; }

        public ExporterRef(IExporter p)
        {
            _exporter = p;
        }
    }

    public abstract class ExporterRefHookable<T> : ExporterRef
    {
        protected List<T> Hooks = new List<T>();

        public ExporterRefHookable(IExporter parent)
            : base(parent)
        {
        }

        public void AddHook(T hook)
        {
            Hooks.Add(hook);
        }
    }
}
