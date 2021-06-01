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
        public override void PostHook(Exporter exporter, Transform trans)
        {
            var gltf = exporter.Context.Gltf;

            var extVrm = new Types.Vrm();
            extVrm.ExporterVersion = "VGltf.Ext.Vrm0";

            ExportMeta(exporter, extVrm, trans);
            ExportHumanoid(exporter, extVrm, trans);
            ExportMaterial(exporter, extVrm);

            //
            gltf.AddExtension(Types.Vrm.ExtensionName, extVrm);
            gltf.AddExtensionUsed(Types.Vrm.ExtensionName);
        }

        void ExportMeta(Exporter exporter, Types.Vrm extVrm, Transform trans)
        {
            var go = trans.gameObject;

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
            vrmMeta.Texture = -1; // ???
            vrmMeta.AllowedUserName = meta.AllowedUserName;
            vrmMeta.ViolentUsage = meta.ViolentUsage;
            vrmMeta.SexualUsage = meta.SexualUsage;
            vrmMeta.CommercialUsage = meta.CommercialUsage;
            vrmMeta.OtherPermissionUrl = meta.OtherPermissionUrl;
            vrmMeta.License = meta.License;
            vrmMeta.OtherLicenseUrl = meta.OtherLicenseUrl;

            extVrm.Meta = vrmMeta;
        }

        void ExportHumanoid(Exporter exporter, Types.Vrm extVrm, Transform trans)
        {
            var go = trans.gameObject;

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
                var boneNode = exporter.Context.RuntimeResources.Nodes[boneTrans];
                vrmHumBone.Node = boneNode.Index;

                return vrmHumBone;
            }).ToList();

            extVrm.Humanoid = vrmHum;
        }

        void ExportMaterial(Exporter exporter, Types.Vrm extVrm)
        {
            var vrmMats = exporter.Context.RuntimeResources.Materials.Map(mat =>
            {
                var vrmMat = new Types.Material();

                // TODO: if mat.shader is MToon, support that

                vrmMat.Name = mat.Value.name;
                vrmMat.Shader = Types.Material.VRM_USE_GLTFSHADER;

                return (mat.Index, vrmMat);
            }).OrderBy(tup => tup.Index).Select(tup => tup.vrmMat).ToList();

            extVrm.MaterialProperties = vrmMats;
        }
    }
}
