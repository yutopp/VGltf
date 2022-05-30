using System;
using System.Collections.Generic;
using UnityEngine;
using VGltf.Unity;

namespace VGltfExamples.VRMExample
{
    public sealed class VRM0ExporterBridge : VGltf.Ext.Vrm0.Unity.Bridge.IExporterBridge
    {
        readonly VGltf.Ext.Vrm0.Unity.DefaultExporterBridge _defaultBridge = new VGltf.Ext.Vrm0.Unity.DefaultExporterBridge();

        public void ExportMeta(Exporter exporter, VGltf.Ext.Vrm0.Types.Vrm vrm, GameObject go)
        {
            _defaultBridge.ExportMeta(exporter, vrm, go);
        }

        public void ExportFirstPerson(IExporterContext context, VGltf.Ext.Vrm0.Types.Vrm extVrm, GameObject go)
        {
            _defaultBridge.ExportFirstPerson(context, extVrm, go);
        }

        public void ExportBlendShapeMaster(Exporter exporter, VGltf.Ext.Vrm0.Types.Vrm vrm, GameObject go)
        {
            _defaultBridge.ExportBlendShapeMaster(exporter, vrm, go);
        }

        public void ExportSecondaryAnimation(IExporterContext context, VGltf.Ext.Vrm0.Types.Vrm extVrm, GameObject go)
        {
            _defaultBridge.ExportSecondaryAnimation(context, extVrm, go);
        }

        public VGltf.Ext.Vrm0.Types.Material CreateMaterialProp(IExporterContext context, Material mat)
        {
            switch (mat.shader.name)
            {
                case MToon.Utils.ShaderName:
                    return CreateMaterialPropForMToon(context, mat);
                default:
                    return _defaultBridge.CreateMaterialProp(context, mat);
            }
        }

        VGltf.Ext.Vrm0.Types.Material CreateMaterialPropForMToon(IExporterContext context, Material mat)
        {
            var vrmMat = new VGltf.Ext.Vrm0.Types.Material();

            vrmMat.Name = mat.name;
            vrmMat.Shader = MToon.Utils.ShaderName;
            vrmMat.RenderQueue = mat.renderQueue;

            foreach (var keyword in mat.shaderKeywords)
            {
                vrmMat.KeywordMap.Add(keyword, mat.IsKeywordEnabled(keyword));
            }
            foreach (var tag in MToonProps.Tags)
            {
                var v = mat.GetTag(tag, false);
                if (!string.IsNullOrEmpty(v))
                {
                    vrmMat.TagMap.Add(tag, v);
                }
            }
            foreach (var prop in MToonProps.Props)
            {
                switch (prop.Value)
                {
                    case MToonProps.PropKind.Float:
                        {
                            var v = mat.GetFloat(prop.Key);
                            vrmMat.FloatProperties.Add(prop.Key, v);
                            break;
                        }

                    case MToonProps.PropKind.Color:
                        {
                            var v = mat.GetColor(prop.Key);
                            // color space conversion required?
                            vrmMat.VectorProperties.Add(prop.Key, new float[] { v.r, v.g, v.b, v.a });
                            break;
                        }

                    case MToonProps.PropKind.Tex:
                        {
                            var v = mat.GetTexture(prop.Key);
                            if (v == null)
                            {
                                continue;
                            }
                            var vRes = context.Exporters.Textures.Export(v);
                            vrmMat.TextureProperties.Add(prop.Key, vRes.Index);
                            break;
                        }
                }
            }

            return vrmMat;
        }
    }
}
