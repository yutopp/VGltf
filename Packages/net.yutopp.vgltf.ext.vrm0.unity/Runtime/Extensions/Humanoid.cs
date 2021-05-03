//
// Copyright (c) 2021 - yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using UnityEngine;

namespace VGltf.Ext.Vrm0.Unity.Extensions
{
    public static class HumanoidBoneTypeBoneEnumExtensions
    {
        public static string ToUnity(this VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum e)
        {
            switch (e)
            {
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.hips:
                    return "Hips";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftUpperLeg:
                    return "LeftUpperLeg";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightUpperLeg:
                    return "RightUpperLeg";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftLowerLeg:
                    return "LeftLowerLeg";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightLowerLeg:
                    return "RightLowerLeg";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftFoot:
                    return "LeftFoot";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightFoot:
                    return "RightFoot";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.spine:
                    return "Spine";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.chest:
                    return "Chest";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.neck:
                    return "Neck";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.head:
                    return "Head";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftShoulder:
                    return "LeftShoulder";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightShoulder:
                    return "RightShoulder";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftUpperArm:
                    return "LeftUpperArm";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightUpperArm:
                    return "RightUpperArm";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftLowerArm:
                    return "LeftLowerArm";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightLowerArm:
                    return "RightLowerArm";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftHand:
                    return "LeftHand";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightHand:
                    return "RightHand";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftToes:
                    return "LeftToes";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightToes:
                    return "RightToes";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftEye:
                    return "LeftEye";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightEye:
                    return "RightEye";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.jaw:
                    return "Jaw";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftThumbProximal:
                    return "LeftThumbProximal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftThumbIntermediate:
                    return "LeftThumbIntermediate";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftThumbDistal:
                    return "LeftThumbDistal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftIndexProximal:
                    return "LeftIndexProximal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftIndexIntermediate:
                    return "LeftIndexIntermediate";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftIndexDistal:
                    return "LeftIndexDistal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftMiddleProximal:
                    return "LeftMiddleProximal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftMiddleIntermediate:
                    return "LeftMiddleIntermediate";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftMiddleDistal:
                    return "LeftMiddleDistal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftRingProximal:
                    return "LeftRingProximal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftRingIntermediate:
                    return "LeftRingIntermediate";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftRingDistal:
                    return "LeftRingDistal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftLittleProximal:
                    return "LeftLittleProximal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftLittleIntermediate:
                    return "LeftLittleIntermediate";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.leftLittleDistal:
                    return "LeftLittleDistal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightThumbProximal:
                    return "RightThumbProximal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightThumbIntermediate:
                    return "RightThumbIntermediate";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightThumbDistal:
                    return "RightThumbDistal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightIndexProximal:
                    return "RightIndexProximal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightIndexIntermediate:
                    return "RightIndexIntermediate";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightIndexDistal:
                    return "RightIndexDistal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightMiddleProximal:
                    return "RightMiddleProximal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightMiddleIntermediate:
                    return "RightMiddleIntermediate";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightMiddleDistal:
                    return "RightMiddleDistal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightRingProximal:
                    return "RightRingProximal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightRingIntermediate:
                    return "RightRingIntermediate";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightRingDistal:
                    return "RightRingDistal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightLittleProximal:
                    return "RightLittleProximal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightLittleIntermediate:
                    return "RightLittleIntermediate";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.rightLittleDistal:
                    return "RightLittleDistal";
                case VGltf.Ext.Vrm0.Types.Humanoid.BoneType.BoneEnum.upperChest:
                    return "UpperChest";
                default:
                    throw new ArgumentException();
            }
        }
    }
}
