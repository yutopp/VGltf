using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VGltf;
using VGltf.Unity;

namespace VGltfExamples.VRMExample
{
    public sealed class VRM0ImporterBridge : VGltf.Ext.Vrm0.Unity.Bridge.IImporterBridge
    {
        public void ReplaceMaterialByMtoon(Importer importer, IndexedResource<Material> matRes, VGltf.Ext.Vrm0.Types.Material matProp)
        {
            if (matProp.Shader == VGltf.Ext.Vrm0.Types.Material.VRM_USE_GLTFSHADER)
            {
                // DO nothing!
                return;
            }

            var shader = Shader.Find(MToon.Utils.ShaderName);
            if (shader == null)
            {
                throw new Exception("VRM0 shader is not found");
            }

            var mat = matRes.Value;

            mat.shader = shader;
            mat.renderQueue = matProp.RenderQueue;
            foreach (var kv in matProp.FloatProperties)
            {
                mat.SetFloat(kv.Key, kv.Value);
            }
            foreach (var kv in matProp.KeywordMap)
            {
                if (kv.Value)
                {
                    mat.EnableKeyword(kv.Key);
                }
                else
                {
                    mat.DisableKeyword(kv.Key);
                }
            }
            foreach (var kv in matProp.TagMap)
            {
                mat.SetOverrideTag(kv.Key, kv.Value);
            }
            foreach (var kv in matProp.TextureProperties)
            {
                if (!importer.Context.Resources.Textures.TryGetValue(kv.Value, out var texRes))
                {
                    texRes = importer.Context.Importers.Textures.Import(kv.Value);
                }
                mat.SetTexture(kv.Key, texRes.Value);
            }
            foreach (var kv in matProp.VectorProperties)
            {
                if (matProp.TextureProperties.ContainsKey(kv.Key))
                {
                    continue;
                }
                mat.SetVector(kv.Key, new Vector4(kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3]));
            }
        }
    }
}