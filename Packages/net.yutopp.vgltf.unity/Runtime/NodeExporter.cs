//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VGltf.Types.Extensions;

namespace VGltf.Unity
{
    public abstract class NodeExporterHook
    {
        public virtual void PostHook(NodeExporter exporter, Transform trans, Types.Node gltfNode)
        {
        }
    }

    public class NodeExporter : ExporterRefHookable<NodeExporterHook>
    {
        public override IExporterContext Context { get; }

        CoordUtils _coordUtils;

        public NodeExporter(IExporterContext context, CoordUtils coordUtils)
        {
            Context = context;
            _coordUtils = coordUtils;
        }

        public IndexedResource<Transform> Export(GameObject go)
        {
            return Export(go.transform);
        }

        public IndexedResource<Transform> Export(Transform trans)
        {
            return Context.RuntimeResources.Nodes.GetOrCall(trans, () =>
            {
                return ForceExport(trans);
            });
        }

        public IndexedResource<Transform> ForceExport(Transform trans)
        {
            var go = trans.gameObject;

            IndexedResource<Mesh> meshResource = null;
            int? skinIndex = null;
            var mr = go.GetComponent<MeshRenderer>();
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (mr != null)
            {
                var meshFilter = mr.gameObject.GetComponent<MeshFilter>();
                var sharedMesh = meshFilter.sharedMesh;

                meshResource = Context.Meshes.Export(mr, sharedMesh);
            }
            else if (smr != null)
            {
                var sharedMesh = smr.sharedMesh;
                meshResource = Context.Meshes.Export(smr, sharedMesh);

                if (smr.bones.Length > 0)
                {
                    skinIndex = ExportSkin(smr, sharedMesh).Index;
                }
            }

            var t = _coordUtils.ConvertSpace(go.transform.localPosition);
            var r = _coordUtils.ConvertSpace(go.transform.localRotation);
            var s = go.transform.localScale;

            var gltfNode = new Types.Node
            {
                Name = go.name,

                Mesh = meshResource != null ? (int?)meshResource.Index : null,
                Skin = skinIndex,

                Matrix = null,
                Translation = new float[] { t.x, t.y, t.z },
                Rotation = new float[] { r.x, r.y, r.z, r.w },
                Scale = new float[] { s.x, s.y, s.z },
            };

            var nodeIndex = Context.Gltf.AddNode(gltfNode);
            var resource = Context.RuntimeResources.Nodes.Add(trans, nodeIndex, go.name, trans);

            var nodesIndices = new List<int>();
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                var c = go.transform.GetChild(i);
                var nodeResource = Export(c.gameObject);
                nodesIndices.Add(nodeResource.Index);
            }
            if (nodesIndices.Count > 0)
            {
                gltfNode.Children = nodesIndices.ToArray();
            }

            foreach (var h in Hooks)
            {
                h.PostHook(this, trans, gltfNode);
            }

            return resource;
        }

        public IndexedResource<Skin> ExportSkin(SkinnedMeshRenderer r, Mesh mesh)
        {
            return Context.RuntimeResources.Skins.GetOrCall(mesh, () =>
            {
                return ForceExportSkin(r, mesh);
            });
        }

        IndexedResource<Skin> ForceExportSkin(SkinnedMeshRenderer smr, Mesh mesh)
        {
            var rootBone = smr.rootBone != null ? (int?)Export(smr.rootBone).Index : null;

            var boneTransValues = smr.bones.Select(bt => Export(bt)).ToArray();
            var boneIndices = boneTransValues.Select(t => t.Index).ToArray();

            int? matricesAccIndex = null;
            if (mesh.bindposes.Length > 0)
            {
                matricesAccIndex = ExportInverseBindMatrices(mesh.bindposes);
            }

            var gltfSkin = new Types.Skin
            {
                InverseBindMatrices = matricesAccIndex,
                Skeleton = rootBone,
                Joints = boneIndices,
            };
            var skinIndex = Context.Gltf.AddSkin(gltfSkin);
            var resource = Context.RuntimeResources.Skins.Add(mesh, skinIndex, mesh.name, new Skin());

            return resource;
        }

        // https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#skin

        int ExportInverseBindMatrices(Matrix4x4[] matrices)
        {
            matrices = matrices.Select(_coordUtils.ConvertSpace).ToArray();

            // MAT4! | FLOAT!
            byte[] buffer = PrimitiveExporter.Marshal(matrices);
            var viewIndex = Context.BufferBuilder.AddView(new ArraySegment<byte>(buffer));

            var accessor = new Types.Accessor
            {
                BufferView = viewIndex,
                ByteOffset = 0,
                ComponentType = Types.Accessor.ComponentTypeEnum.FLOAT,
                Count = matrices.Length,
                Type = Types.Accessor.TypeEnum.Mat4,
            };
            return Context.Gltf.AddAccessor(accessor);
        }
    }
}
