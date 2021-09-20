//
// Copyright (c) 2021 - yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VGltf.Types.Extensions;
using VGltf.Unity;
using VGltf.Ext.Vrm0.Unity.Extensions;

namespace VGltf.Ext.Vrm0.Unity.Hooks
{
    public class ImporterHook : ImporterHookBase
    {
        readonly GameObject _rootGo;
        readonly Bridge.IImporterBridge _bridge;

        public ImporterHook(GameObject rootGo, Bridge.IImporterBridge bridge)
        {
            _rootGo = rootGo;
            _bridge = bridge;
        }

        public override void PostHook(Importer importer)
        {
            var gltf = importer.Context.Container.Gltf;

            if (!gltf.ContainsExtensionUsed(Types.Vrm.ExtensionName))
            {
                throw new Exception($"VRM extension name `{Types.Vrm.ExtensionName}` is not contained");
            }

            // TODO: migration
            if (!gltf.TryGetExtension<Types.Vrm>(Types.Vrm.ExtensionName, out var vrm))
            {
                throw new Exception("No vrm extension record");
            }

            ImportMeta(importer, vrm, _rootGo);
            ImportHumanoid(importer.Context, vrm);
            // firstPerson
            ImportBlendShapeMaster(importer, vrm, _rootGo);
            // secondaryAnimation
            ImportMaterial(importer, vrm);
        }

        void ImportMeta(Importer importer, Types.Vrm vrm, GameObject go)
        {
            _bridge.ImportMeta(importer, vrm.Meta, go);
        }

        void ImportHumanoid(IImporterContext context, Types.Vrm vrm)
        {
            // VRM glTF structures.
            // - scenes
            //   - nodes
            //     - armature
            //     - gameobjects...
            // Thus all nodes under scenes should be reconstructed as children of root game object.

            var gltfScene = context.Container.Gltf.GetSceneObject();
            var childrenNodes = gltfScene.Nodes.Select(nodeIndex => context.Resources.Nodes[nodeIndex].Value.transform);

            foreach (var childrenNode in childrenNodes)
            {
                childrenNode.SetParent(_rootGo.transform, false);
            }

            var vrmHum = vrm.Humanoid;

            var hd = new HumanDescription();

            hd.upperArmTwist = vrmHum.UpperArmTwist;
            hd.lowerArmTwist = vrmHum.LowerArmTwist;
            hd.upperLegTwist = vrmHum.UpperLegTwist;
            hd.lowerLegTwist = vrmHum.LowerLegTwist;
            hd.armStretch = vrmHum.ArmStretch;
            hd.legStretch = vrmHum.LegStretch;
            hd.feetSpacing = vrmHum.FeetSpacing;
            hd.hasTranslationDoF = vrmHum.HasTranslationDoF;

            // NOTE: Maybe VRM humanoid is a broken format.
            // There is not enough information in HumanBones, so we need to scan all the data (recursively).
            // It's going to get weird when we get duplicate names.
            var allNodes = _rootGo.GetComponentsInChildren<Transform>().Where(n =>
            {
                return
                    (n.GetComponent<MeshRenderer>() == null) &&
                    (n.GetComponent<SkinnedMeshRenderer>() == null);
            });
            hd.skeleton = allNodes.Select(n =>
            {
                return new SkeletonBone
                {
                    name = n.name,
                    position = n.localPosition,
                    rotation = n.localRotation,
                    scale = n.localScale,
                };
            }).ToArray();

            hd.human = vrmHum.HumanBones.Select(vrmHumBone =>
            {
                var limit = new HumanLimit
                {
                    useDefaultValues = vrmHumBone.UseDefaultValues,
                    min = vrmHumBone.Min.ToUnity(),
                    max = vrmHumBone.Max.ToUnity(),
                    center = vrmHumBone.Center.ToUnity(),
                    axisLength = vrmHumBone.AxisLength,
                };

                var node = context.Resources.Nodes[vrmHumBone.Node];

                return new HumanBone
                {
                    boneName = node.Value.name,
                    humanName = vrmHumBone.Bone.ToUnity(),
                    limit = limit,
                };
            }).ToArray();

            var go = _rootGo;
            Avatar avater = null;
            avater = AvatarBuilder.BuildHumanAvatar(go, hd);

            var anim = go.AddComponent<Animator>();
            anim.avatar = avater;

            // Raise exceptions after showing warning logs by Unity.
            if (!avater.isValid || !avater.isHuman)
            {
                throw new Exception("Avatar is invalid or not human");
            }
        }

        void ImportBlendShapeMaster(Importer importer, Types.Vrm vrm, GameObject go)
        {
            _bridge.ImportBlendShapeMaster(importer, vrm.BlendShapeMaster, go);
        }

        void ImportMaterial(Importer importer, Types.Vrm vrm)
        {
            foreach (var matProp in vrm.MaterialProperties)
            {
                if (!importer.Context.Resources.Materials.TryGetValueByName(matProp.Name, out var matRes))
                {
                    throw new Exception($"VRM0 material is not found: name={matProp.Name}");
                }

                _bridge.ReplaceMaterialByMtoon(importer, matProp, matRes);
            }
        }
    }
}
