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
    public abstract class MaterialImporterHook
    {
        public abstract IndexedResource<Material> Import(MaterialImporter importer, int matIndex);
    }

    public class MaterialImporter : ImporterRefHookable<MaterialImporterHook>
    {
        public MaterialImporter(Importer parent)
            : base(parent)
        {
        }

        public IndexedResource<Material> Import(int matIndex)
        {
            var gltf = Container.Gltf;
            var gltfMat = gltf.Materials[matIndex];

            return Cache.CacheObjectIfNotExists(matIndex, matIndex, Cache.Materials, ForceImport);
        }

        public IndexedResource<Material> ForceImport(int matIndex)
        {
            foreach(var h in Hooks)
            {
                var r = h.Import(this, matIndex);
                if (r != null)
                {
                    return r;
                }
            }

            // Default import
            var gltf = Container.Gltf;
            var gltfMat = gltf.Materials[matIndex];

            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                throw new NotImplementedException();
            }

            var mat = new Material(shader);
            mat.name = gltfMat.Name;

            if (gltfMat.PbrMetallicRoughness != null)
            {
                var pbrMR = gltfMat.PbrMetallicRoughness;
                if (pbrMR.BaseColorTexture != null)
                {
                    var bct = pbrMR.BaseColorTexture;
                    var textureResource = Textures.Import(bct.Index);
                    mat.SetTexture("_MainTex", textureResource.Value);
                }
            }

            return new IndexedResource<Material>
            {
                Index = matIndex,
                Value = mat,
            };
        }
    }
}
