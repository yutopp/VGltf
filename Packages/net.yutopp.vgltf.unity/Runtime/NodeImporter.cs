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
        public virtual void PostHook(NodeImporter impoter, Transform trans, Types.Node gltfNode)
        {
        }
    }

    public class NodeImporter : ImporterRefHookable<NodeImporterHook>
    {
        public NodeImporter(Importer parent)
            : base(parent)
        {
        }

        public void ImportGameObjects(int nodeIndex, NodesCache gameObjects, GameObject parentGo)
        {
            var gltf = Container.Gltf;
            var gltfNode = gltf.Nodes[nodeIndex];

            var go = new GameObject();
            if (parentGo != null)
            {
                go.transform.SetParent(parentGo.transform, false);
            }
            go.name = gltfNode.Name;

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

            gameObjects.Add(nodeIndex, go);

            if (gltfNode.Children != null)
            {
                foreach (var childIndex in gltfNode.Children)
                {
                    if (gameObjects.Contains(childIndex))
                    {
                        throw new NotImplementedException("Node duplication"); // TODO:
                    }
                    ImportGameObjects(childIndex, gameObjects, go);
                }
            }
        }

        public void ImportMeshesAndSkins(int nodeIndex, NodesCache gameObjects)
        {
            var gltf = Container.Gltf;
            var gltfNode = gltf.Nodes[nodeIndex];
            var go = gameObjects.Get(nodeIndex);

            if (gltfNode.Mesh != null)
            {
                var meshResource = Meshes.Import(gltfNode.Mesh.Value, go);

                if (gltfNode.Skin != null)
                {
                    ImportSkin(gltfNode.Skin.Value, gameObjects, go);
                }
            }

            if (gltfNode.Children != null)
            {
                foreach (var childIndex in gltfNode.Children)
                {
                    ImportMeshesAndSkins(childIndex, gameObjects);
                }
            }

            // TODO: move to elsewhere...
            foreach (var h in Hooks)
            {
                h.PostHook(this, go.transform, gltfNode);
            }
        }

        void ImportSkin(int skinIndex, NodesCache gameObjects, GameObject go)
        {
            var gltf = Container.Gltf;
            var gltfSkin = gltf.Skins[skinIndex];

            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr == null)
            {
                throw new NotImplementedException(); // TODO
            }

            if (gltfSkin.Skeleton != null)
            {
                smr.rootBone = gameObjects.Get(gltfSkin.Skeleton.Value).transform;
            }

            smr.bones = gltfSkin.Joints.Select(i => gameObjects.Get(i).transform).ToArray();

            if (gltfSkin.InverseBindMatrices != null)
            {
                var buf = BufferView.GetOrLoadTypedBufferByAccessorIndex(gltfSkin.InverseBindMatrices.Value);
                var matrices = buf.GetEntity<Matrix4x4>().GetEnumerable().Select(CoordUtils.ConvertSpace).ToArray();

                var mesh = smr.sharedMesh;
                mesh.bindposes = matrices;
            }
        }
    }

    public class NodesCache
    {
        readonly Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

        public void Add(int nodeIndex, GameObject go)
        {
            _objects.Add(nodeIndex, go);
        }

        public bool Contains(int nodeIndex)
        {
            return _objects.ContainsKey(nodeIndex);
        }

        public GameObject Get(int nodeIndex)
        {
            return _objects[nodeIndex];
        }
    }
}
