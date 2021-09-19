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
    public sealed class DefaultImporterBridge : VGltf.Ext.Vrm0.Unity.Bridge.IImporterBridge
    {
        public void ImportMeta(Importer importer, VGltf.Ext.Vrm0.Types.Meta vrmMeta, GameObject go)
        {
            var meta = go.AddComponent<VRM0Meta>();

            meta.Title = vrmMeta.Title;
            meta.Version = vrmMeta.Version;
            meta.Author = vrmMeta.Author;
            meta.ContactInformation = vrmMeta.ContactInformation;
            meta.Reference = vrmMeta.Reference;
            // meta.Texture = vrmMeta.Texture; // TODO: support
            meta.AllowedUserName = vrmMeta.AllowedUserName;
            meta.ViolentUsage = vrmMeta.ViolentUsage;
            meta.SexualUsage = vrmMeta.SexualUsage;
            meta.CommercialUsage = vrmMeta.CommercialUsage;
            meta.OtherPermissionUrl = vrmMeta.OtherPermissionUrl;
            meta.License = vrmMeta.License;
            meta.OtherLicenseUrl = vrmMeta.OtherLicenseUrl;
        }

        public void ReplaceMaterialByMtoon(Importer importer, VGltf.Ext.Vrm0.Types.Material matProp, IndexedResource<Material> matRes)
        {
            throw new NotImplementedException();
        }
    }
}