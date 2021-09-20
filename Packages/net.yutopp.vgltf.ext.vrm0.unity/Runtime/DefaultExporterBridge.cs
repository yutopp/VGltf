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

        public Types.Material CreateMaterialPropForMToon(Exporter exporter, VGltf.Ext.Vrm0.Types.Vrm vrm, IndexedResource<Material> matRes)
        {
            throw new NotImplementedException();
        }
    }
}