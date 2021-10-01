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
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VGltf.Unity
{
    public abstract class MaterialImporterHook
    {
        public abstract Task<IndexedResource<Material>> Import(IImporterContext context, int matIndex, CancellationToken ct);
    }

    public class MaterialImporter : ImporterRefHookable<MaterialImporterHook>
    {
        public override IImporterContext Context { get; }

        public MaterialImporter(IImporterContext context)
        {
            Context = context;
        }

        public async Task<IndexedResource<Material>> Import(int matIndex, CancellationToken ct)
        {
            var gltf = Context.Container.Gltf;
            var gltfMat = gltf.Materials[matIndex];

            return await Context.Resources.Materials.GetOrCallAsync(matIndex, async () =>
            {
                return await ForceImport(matIndex, ct);
            });
        }

        public async Task<IndexedResource<Material>> ForceImport(int matIndex, CancellationToken ct)
        {
            foreach (var h in Hooks)
            {
                var r = await h.Import(Context, matIndex, ct);
                if (r != null)
                {
                    return r;
                }
            }

            // Default import
            var gltf = Context.Container.Gltf;
            var gltfMat = gltf.Materials[matIndex];

            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                throw new Exception($"Standard shader is not found");
            }

            var mat = new Material(shader);
            mat.name = gltfMat.Name;

            var resource = Context.Resources.Materials.Add(matIndex, matIndex, mat.name, mat);

            if (gltfMat.PbrMetallicRoughness != null)
            {
                var pbrMR = gltfMat.PbrMetallicRoughness;
                if (pbrMR.BaseColorTexture != null)
                {
                    var bct = pbrMR.BaseColorTexture;
                    var textureResource = await Context.Importers.Textures.Import(bct.Index, ct);
                    mat.SetTexture("_MainTex", textureResource.Value);
                }
            }

            return resource;
        }
    }
}
