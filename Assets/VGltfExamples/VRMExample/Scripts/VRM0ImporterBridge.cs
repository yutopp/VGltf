using System;
using UnityEngine;
using VGltf.Unity;

namespace VGltfExamples.VRMExample
{
    public sealed class VRM0ImporterBridge : VGltf.Ext.Vrm0.Unity.Bridge.IImporterBridge
    {
        readonly VGltf.Ext.Vrm0.Unity.DefaultImporterBridge _defaultBridge = new VGltf.Ext.Vrm0.Unity.DefaultImporterBridge();

        public void ImportMeta(Importer importer, VGltf.Ext.Vrm0.Types.Meta vrmMeta, GameObject go)
        {
            _defaultBridge.ImportMeta(importer, vrmMeta, go);
        }

        public void ImportBlendShapeMaster(Importer importer, VGltf.Ext.Vrm0.Types.BlendShape vrmBlendShape, GameObject go)
        {
            _defaultBridge.ImportBlendShapeMaster(importer, vrmBlendShape, go);
        }

        public void ReplaceMaterialByMtoon(Importer importer, VGltf.Ext.Vrm0.Types.Material matProp, IndexedResource<Material> matRes)
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