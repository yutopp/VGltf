//
// Copyright (c) 2021 - yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using UnityEngine;
using VGltf.Unity;

namespace VGltf.Ext.Vrm0.Unity
{
    public sealed class DefaultExporterBridge : VGltf.Ext.Vrm0.Unity.Bridge.IExporterBridge
    {
        public void ExportMeta(Exporter exporter, VGltf.Ext.Vrm0.Types.Vrm vrm, GameObject go)
        {
            var meta = go.GetComponent<VRM0Meta>();
            if (meta == null)
            {
                throw new Exception("There is no VRM0Meta component");
            }

            var vrmMeta = new Types.Meta();

            vrmMeta.Title = meta.Title;
            vrmMeta.Version = meta.Version;
            vrmMeta.Author = meta.Author;
            vrmMeta.ContactInformation = meta.ContactInformation;
            vrmMeta.Reference = meta.Reference;
            vrmMeta.Texture = -1; // ???, TODO: implement
            vrmMeta.AllowedUserName = meta.AllowedUserName;
            vrmMeta.ViolentUsage = meta.ViolentUsage;
            vrmMeta.SexualUsage = meta.SexualUsage;
            vrmMeta.CommercialUsage = meta.CommercialUsage;
            vrmMeta.OtherPermissionUrl = meta.OtherPermissionUrl;
            vrmMeta.License = meta.License;
            vrmMeta.OtherLicenseUrl = meta.OtherLicenseUrl;

            vrm.Meta = vrmMeta;
        }

        public void ExportBlendShapeMaster(Exporter exporter, VGltf.Ext.Vrm0.Types.Vrm vrm, GameObject go)
        {
            var proxy = go.GetComponent<VRM0BlendShapeProxy>();
            if (proxy == null)
            {
                // blendshape proxy is optional
                return;
            }

            foreach (var proxyGroup in proxy.Groups)
            {
                var g = new Types.BlendShape.GroupType();
                g.Name = proxyGroup.Name;
                g.PresetName = ToVRM0Preset(proxyGroup.Preset);

                foreach (var shape in proxyGroup.MeshShapes)
                {
                    var smr = shape.SkinnedMeshRenderer;
                    if (!exporter.Context.Resources.Meshes.TryGetValueByName(smr.sharedMesh.name, out var mesh))
                    {
                        continue;
                    }

                    foreach (var weight in shape.Weights)
                    {
                        var index = mesh.Value.GetBlendShapeIndex(weight.ShapeKeyName);
                        if (index == -1)
                        {
                            continue;
                        }

                        g.Binds.Add(new Types.BlendShape.BindType
                        {
                            Mesh = mesh.Index,
                            Index = index,
                            Weight = weight.WeightValue,
                        });
                    }
                }

                vrm.BlendShapeMaster.BlendShapeGroups.Add(g);
            }
        }

        public Types.Material CreateMaterialProp(Exporter exporter, VGltf.Ext.Vrm0.Types.Vrm vrm, IndexedResource<Material> matRes)
        {
            var vrmMat = new Types.Material();

            // TODO: if mat.shader is MToon, support that

            vrmMat.Name = matRes.Value.name;
            vrmMat.Shader = Types.Material.VRM_USE_GLTFSHADER;

            return vrmMat;
        }

        static Types.BlendShape.GroupType.BlendShapePresetEnum ToVRM0Preset(VRM0BlendShapeProxy.BlendShapePreset kind)
        {
            switch (kind)
            {
                case VRM0BlendShapeProxy.BlendShapePreset.Unknown:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.Unknown;

                case VRM0BlendShapeProxy.BlendShapePreset.Neutral:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.Neutral;

                case VRM0BlendShapeProxy.BlendShapePreset.A:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.A;
                case VRM0BlendShapeProxy.BlendShapePreset.I:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.I;
                case VRM0BlendShapeProxy.BlendShapePreset.U:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.U;
                case VRM0BlendShapeProxy.BlendShapePreset.E:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.E;
                case VRM0BlendShapeProxy.BlendShapePreset.O:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.O;

                case VRM0BlendShapeProxy.BlendShapePreset.Blink:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.Blink;

                case VRM0BlendShapeProxy.BlendShapePreset.Joy:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.Joy;
                case VRM0BlendShapeProxy.BlendShapePreset.Angry:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.Angry;
                case VRM0BlendShapeProxy.BlendShapePreset.Sorrow:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.Sorrow;
                case VRM0BlendShapeProxy.BlendShapePreset.Fun:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.Fun;

                case VRM0BlendShapeProxy.BlendShapePreset.LookUp:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.LookUp;
                case VRM0BlendShapeProxy.BlendShapePreset.LookDown:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.LookDown;
                case VRM0BlendShapeProxy.BlendShapePreset.LookLeft:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.LookLeft;
                case VRM0BlendShapeProxy.BlendShapePreset.LookRight:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.LookRight;

                case VRM0BlendShapeProxy.BlendShapePreset.Blink_L:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.Blink_L;
                case VRM0BlendShapeProxy.BlendShapePreset.Blink_R:
                    return Types.BlendShape.GroupType.BlendShapePresetEnum.Blink_R;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
