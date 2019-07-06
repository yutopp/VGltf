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
    public class NodeImporter : ImporterRef
    {
        public NodeImporter(Importer parent)
            : base(parent)
        {
        }

        public GameObject Import(int nodeIndex)
        {
            var gameObjects = new Dictionary<int, GameObject>();
            CreateGameObjects(nodeIndex, gameObjects);
            ImportMeshesAndSkins(nodeIndex, gameObjects);

            return gameObjects[nodeIndex];
        }

        void CreateGameObjects(int nodeIndex, Dictionary<int, GameObject> gameObjects)
        {
            var gltf = Container.Gltf;
            var gltfNode = gltf.Nodes[nodeIndex];

            var go = new GameObject();
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
                    if (gameObjects.ContainsKey(childIndex))
                    {
                        throw new NotImplementedException("Node duplication"); // TODO:
                    }
                    CreateGameObjects(childIndex, gameObjects);

                    var childGo = gameObjects[childIndex];
                    childGo.transform.SetParent(go.transform, false);
                }
            }
        }

        void ImportMeshesAndSkins(int nodeIndex, Dictionary<int, GameObject> gameObjects)
        {
            var gltf = Container.Gltf;
            var gltfNode = gltf.Nodes[nodeIndex];
            var go = gameObjects[nodeIndex];

            if (gltfNode.Mesh != null)
            {
                var meshResource = Meshes.Import(gltfNode.Mesh.Value, go);

                if (gltfNode.Skin != null)
                {
                    ImportSkin(gltfNode.Skin.Value, go, gameObjects);
                }
            }

            if (gltfNode.Children != null)
            {
                foreach (var childIndex in gltfNode.Children)
                {
                    ImportMeshesAndSkins(childIndex, gameObjects);
                }
            }
        }

        void ImportSkin(int skinIndex, GameObject go, Dictionary<int, GameObject> gameObjects)
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
                smr.rootBone = gameObjects[gltfSkin.Skeleton.Value].transform;
            }

            smr.bones = gltfSkin.Joints.Select(i => gameObjects[i].transform).ToArray();

            if (gltfSkin.InverseBindMatrices != null)
            {
                var buf = BufferView.GetOrLoadTypedBufferByAccessorIndex(gltfSkin.InverseBindMatrices.Value);
                var matrices = buf.GetEntity<Matrix4x4>().GetEnumerable().Select(CoordUtils.ConvertSpace).ToArray();

                var mesh = smr.sharedMesh;
                mesh.bindposes = matrices;
            }
        }
    }
}
