//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.Linq;
using UnityEngine;

namespace VGltf.Unity.Ext
{
    using GE = VGltf.Types.GltfExtensions;

    public class AvatarImporter : NodeImporterHook
    {
        public override void PostHook(NodeImporter impoter, Transform trans, VGltf.Types.Node gltfNode)
        {
            if (!GE.ContainsExtensionUsed(impoter.Container.Gltf, AvatarType.ExtensionName))
            {
                return;
            }

            AvatarType extAvatar;
            if (!gltfNode.GetExtension(AvatarType.ExtensionName, out extAvatar))
            {
                return;
            }

            var extHD = extAvatar.HumanDescription;

            var hd = new HumanDescription();

            hd.upperArmTwist = extHD.UpperArmTwist;
            hd.lowerArmTwist = extHD.LowerArmTwist;
            hd.upperLegTwist = extHD.UpperLegTwist;
            hd.lowerLegTwist = extHD.LowerLegTwist;
            hd.armStretch = extHD.ArmStretch;
            hd.legStretch = extHD.LegStretch;
            hd.feetSpacing = extHD.FeetSpacing;

            hd.skeleton = extHD.Skeleton.Select(s =>
            {
                // TODO: Coord
                return new SkeletonBone
                {
                    name = s.Name,
                    position = PrimitiveImporter.AsVector3(s.Position),
                    rotation = PrimitiveImporter.AsQuaternion(s.Rotation),
                    scale = PrimitiveImporter.AsVector3(s.Scale),
                };
            }).ToArray();

            hd.human = extHD.Human.Select(h =>
            {
                var extLimit = h.Limit;
                var limit = new HumanLimit
                {
                    useDefaultValues = extLimit.UseDefaultValues,
                    min = PrimitiveImporter.AsVector3(extLimit.Min),
                    max = PrimitiveImporter.AsVector3(extLimit.Max),
                    center = PrimitiveImporter.AsVector3(extLimit.Center),
                    axisLength = extLimit.AxisLength,
                };

                return new HumanBone
                {
                    boneName = h.BoneName,
                    humanName = h.HumanName,
                    limit = limit,
                };
            }).ToArray();

            var go = trans.gameObject;
            var anim = go.AddComponent<Animator>();
            anim.avatar = AvatarBuilder.BuildHumanAvatar(go, hd);
        }
    }
}
