//
// Copyright (c) 2021 - yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using UnityEngine;
using VGltf.Unity;

namespace VGltf.Ext.Vrm0.Unity.Bridge
{
    public interface IImporterBridge
    {
        void ImportMeta(Importer importer, VGltf.Ext.Vrm0.Types.Meta vrmMeta, GameObject go);
        void ImportBlendShapeMaster(Importer importer, VGltf.Ext.Vrm0.Types.BlendShape vrmBlendShape, GameObject go);
        void ReplaceMaterialByMtoon(IImporterContext context, VGltf.Ext.Vrm0.Types.Material matProp, Material mat);
    }
}
