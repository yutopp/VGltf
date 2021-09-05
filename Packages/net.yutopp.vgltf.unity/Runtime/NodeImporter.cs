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

namespace VGltf.Unity
{
    public abstract class NodeImporterHook
    {
        public virtual void PostHook(NodeImporter importer, int nodeIndex, Transform trans)
        {
        }
    }

    public class NodeImporter : ImporterRefHookable<NodeImporterHook>
    {
        public override IImporterContext Context { get; }

        public NodeImporter(IImporterContext context)
        {
            Context = context;
        }

        public void ImportGameObjects(int nodeIndex, GameObject parentGo = null)
        {
            Context.Resources.Nodes.GetOrCall(nodeIndex, () =>
            {
                return ForceImportGameObjects(nodeIndex, parentGo);
            });
        }

        public IndexedResource<Transform> ForceImportGameObjects(int nodeIndex, GameObject parentGo)
        {
            var gltf = Context.Container.Gltf;
            var gltfNode = gltf.Nodes[nodeIndex];

            var go = new GameObject();
            go.name = gltfNode.Name;

            var resource = Context.Resources.Nodes.Add(nodeIndex, nodeIndex, go.transform);

            if (parentGo != null)
            {
                // TODO: go.transform have parent already, it will error
                go.transform.SetParent(parentGo.transform, false);
            }

            var matrix = PrimitiveImporter.AsMatrix4x4(gltfNode.Matrix);
            if (!matrix.isIdentity)
            {
                throw new NotImplementedException("matrix is not implemented");
            }
            else
            {
                var t = PrimitiveImporter.AsVector3(gltfNode.Translation);
                var r = PrimitiveImporter.AsQuaternion(gltfNode.Rotation);
                var s = PrimitiveImporter.AsVector3(gltfNode.Scale);
                go.transform.localPosition = CoordUtils.ConvertSpace(t);
                go.transform.localRotation = CoordUtils.ConvertSpace(r);
                go.transform.localScale = s;
            }

            if (gltfNode.Children != null)
            {
                foreach (var childIndex in gltfNode.Children)
                {
                    if (Context.Resources.Nodes.Contains(childIndex))
                    {
                        throw new NotImplementedException("Node duplication"); // TODO:
                    }
                    ImportGameObjects(childIndex, go);
                }
            }

            return resource;
        }

        public void ImportMeshesAndSkins(int nodeIndex)
        {
            var gltf = Context.Container.Gltf;
            var gltfNode = gltf.Nodes[nodeIndex];

            var go = Context.Resources.Nodes[nodeIndex].Value.gameObject;

            if (gltfNode.Mesh != null)
            {
                var meshResource = Context.Importers.Meshes.Import(gltfNode.Mesh.Value, go);

                if (gltfNode.Skin != null)
                {
                    ImportSkin(gltfNode.Skin.Value, go);
                }
            }

            if (gltfNode.Children != null)
            {
                foreach (var childIndex in gltfNode.Children)
                {
                    ImportMeshesAndSkins(childIndex);
                }
            }

            // TODO: move to elsewhere...
            foreach (var h in Hooks)
            {
                h.PostHook(this, nodeIndex, go.transform);
            }
        }

        void ImportSkin(int skinIndex, GameObject go)
        {
            var gltf = Context.Container.Gltf;
            var gltfSkin = gltf.Skins[skinIndex];

            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr == null)
            {
                throw new NotImplementedException(); // TODO
            }

            if (gltfSkin.Skeleton != null)
            {
                smr.rootBone = Context.Resources.Nodes[gltfSkin.Skeleton.Value].Value;
            }

            smr.bones = gltfSkin.Joints.Select(i => Context.Resources.Nodes[i].Value).ToArray();

            if (gltfSkin.InverseBindMatrices != null)
            {
                var buf = Context.GltfResources.GetOrLoadTypedBufferByAccessorIndex(gltfSkin.InverseBindMatrices.Value);
                var matrices = buf.GetEntity<Matrix4x4>().GetEnumerable().Select(CoordUtils.ConvertSpace).ToArray();

                var mesh = smr.sharedMesh;
                mesh.bindposes = matrices;
            }
        }
    }
}
