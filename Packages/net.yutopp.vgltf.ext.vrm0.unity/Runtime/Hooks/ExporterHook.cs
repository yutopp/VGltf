//
// Copyright (c) 2021 - yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using UnityEngine;
using VGltf.Unity;
using VGltf.Types.Extensions;
using System;
using System.Linq;
using VGltf.Ext.Vrm0.Unity.Extensions;

namespace VGltf.Ext.Vrm0.Unity.Hooks
{
    public class ExporterHook : ExporterHookBase
    {
        readonly Bridge.IExporterBridge _bridge;

        public ExporterHook(Bridge.IExporterBridge bridge)
        {
            _bridge = bridge;
        }

        public override void PostHook(Exporter exporter, GameObject go)
        {
            var gltf = exporter.Context.Gltf;

            var extVrm = new Types.Vrm();
            extVrm.ExporterVersion = "VGltf.Ext.Vrm0";

            ExportMeta(exporter, extVrm, go);
            ExportHumanoid(exporter, extVrm, go);
            // firstPerson
            ExportBlendShapeMaster(exporter, extVrm, go);
            // secondaryAnimation
            ExportMaterial(exporter, extVrm);

            //
            gltf.AddExtension(Types.Vrm.ExtensionName, extVrm);
            gltf.AddExtensionUsed(Types.Vrm.ExtensionName);
        }

        void ExportMeta(Exporter exporter, Types.Vrm extVrm, GameObject go)
        {
            _bridge.ExportMeta(exporter, extVrm, go);
        }

        void ExportHumanoid(Exporter exporter, Types.Vrm extVrm, GameObject go)
        {
            var anim = go.GetComponent<Animator>();
            if (anim == null)
            {
                throw new Exception("There is no Animation component");
            }

            // NOTE: It may break if there are duplicate names
            var nodeTransMap =
                anim.GetComponentsInChildren<Transform>().Where(n =>
                {
                    return !(
                        (n.GetComponent<MeshRenderer>() != null) ||
                        (n.GetComponent<SkinnedMeshRenderer>() != null)
                    );
                }).ToDictionary(t => t.name);

            var avatar = anim.avatar;
            if (!avatar.isValid || !avatar.isHuman)
            {
                throw new Exception("Avatar is invalid or not human");
            }

            var hd = avatar.humanDescription;

            var vrmHum = new Types.Humanoid();

            vrmHum.UpperArmTwist = hd.upperArmTwist;
            vrmHum.LowerArmTwist = hd.lowerArmTwist;
            vrmHum.UpperLegTwist = hd.upperLegTwist;
            vrmHum.LowerLegTwist = hd.lowerLegTwist;
            vrmHum.ArmStretch = hd.armStretch;
            vrmHum.LegStretch = hd.legStretch;
            vrmHum.FeetSpacing = hd.feetSpacing;
            vrmHum.HasTranslationDoF = hd.hasTranslationDoF;

            vrmHum.HumanBones = hd.human.Select(h =>
            {
                var vrmHumBone = new Types.Humanoid.BoneType();

                // HumanLimit (NOTE: coord is still same as Unity. not GL)
                vrmHumBone.UseDefaultValues = h.limit.useDefaultValues;
                vrmHumBone.Min = h.limit.min.ToVrm();
                vrmHumBone.Max = h.limit.max.ToVrm();
                vrmHumBone.Center = h.limit.center.ToVrm();
                vrmHumBone.AxisLength = h.limit.axisLength;

                // HumanBone
                vrmHumBone.Bone = h.humanName.AsHumanBoneNameToVrm();

                var boneTrans = nodeTransMap[h.boneName];
                if (!exporter.Context.Resources.Nodes.TryGetValueByName(boneTrans.name, out var boneNode))
                {
                    throw new Exception($"bone transform is not found: name={boneTrans.name}");
                }
                vrmHumBone.Node = boneNode.Index;

                return vrmHumBone;
            }).ToList();

            extVrm.Humanoid = vrmHum;
        }

        void ExportBlendShapeMaster(Exporter exporter, Types.Vrm extVrm, GameObject go)
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

                extVrm.BlendShapeMaster.BlendShapeGroups.Add(g);
            }
        }

        void ExportMaterial(Exporter exporter, Types.Vrm extVrm)
        {
            var vrmMats = exporter.Context.Resources.Materials.Map(matRes =>
            {
                var vrmMat = new Types.Material();

                // TODO: if mat.shader is MToon, support that

                vrmMat.Name = matRes.Value.name;
                vrmMat.Shader = Types.Material.VRM_USE_GLTFSHADER;

                return (matRes.Index, vrmMat);
            }).OrderBy(tup => tup.Index).Select(tup => tup.vrmMat).ToList();

            extVrm.MaterialProperties = vrmMats;
        }

        static Types.BlendShape.GroupType.BlendShapePresetEnum ToVRM0Preset(VRM0BlendShapeProxy.BlendShapePreset kind)
        {
            switch (kind)
            {
                case VRM0BlendShapeProxy.BlendShapePreset.Unknown:

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
