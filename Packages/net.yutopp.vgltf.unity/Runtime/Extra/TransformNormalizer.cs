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

namespace VGltf.Unity.Ext
{
    public sealed class TransformNormalizer : IDisposable
    {
        public GameObject Go;
        private Dictionary<Mesh, Mesh> bakedMeshes = new Dictionary<Mesh, Mesh>();

        void IDisposable.Dispose()
        {
            if (Go != null)
            {
                Utils.Destroy(Go);
                Go = null;
            }

            // Destroy baked meshes
            foreach (var kv in bakedMeshes)
            {
                Utils.Destroy(kv.Value);
            }
            bakedMeshes.Clear();
        }

        public void Normalize(GameObject go)
        {
            var nGo = UnityEngine.Object.Instantiate(go) as GameObject;
            try
            {
                Normalized(go, nGo);
            }
            catch (Exception)
            {
                Utils.Destroy(nGo);
                throw;
            }

            Go = nGo;
        }

        void Normalized(GameObject go, GameObject nGo)
        {
            nGo.name = string.Format("{0}(VGltf.Normalized)", go.name);

            nGo.transform.localPosition = Vector3.zero;
            nGo.transform.localRotation = Quaternion.identity;
            nGo.transform.localScale = Vector3.one;

            BakeMeshes(nGo);
            NormalizeTransforms(nGo.transform, Matrix4x4.identity);
            UpdateBonePoses(nGo);
        }

        public void BakeMeshes(GameObject go)
        {
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                var sharedMesh = smr.sharedMesh;

                var weights = sharedMesh.boneWeights.Select(w =>
                {
                    return new BoneWeight
                    {
                        weight0 = w.weight0,
                        weight1 = w.weight1,
                        weight2 = w.weight2,
                        weight3 = w.weight3,
                        boneIndex0 = w.boneIndex0,
                        boneIndex1 = w.boneIndex1,
                        boneIndex2 = w.boneIndex2,
                        boneIndex3 = w.boneIndex3,
                    };
                }).ToArray();

                // Initialize forms
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                if (go.GetComponent<Animator>() != null)
                {
                    GameObject.Destroy(go.GetComponent<Animator>());
                }

                for (var i = 0; i < sharedMesh.blendShapeCount; ++i)
                {
                    smr.SetBlendShapeWeight(i, 0.0f);
                }

                // Bake
                var mesh = new Mesh();
                bakedMeshes.Add(sharedMesh, mesh);

                smr.BakeMesh(mesh);

                mesh.name = string.Format("{0}(VGltf.Baked)", sharedMesh.name);
                mesh.boneWeights = weights;

                var vertices = mesh.vertices;
                var normals = mesh.normals;
                var tangents = mesh.tangents;
                for (var i = 0; i < sharedMesh.blendShapeCount; ++i)
                {
                    var blendShapeName = sharedMesh.GetBlendShapeName(i);
                    var blendShapeFrame = sharedMesh.GetBlendShapeFrameCount(i);
                    var blendShapeWeight = sharedMesh.GetBlendShapeFrameWeight(i, blendShapeFrame - 1);

                    smr.SetBlendShapeWeight(i, blendShapeWeight);
                    var blendShapeMesh = new Mesh();
                    try
                    {
                        smr.BakeMesh(blendShapeMesh);

                        var blendShapeVertices = blendShapeMesh.vertices;
                        var blendShapeNormals = blendShapeMesh.normals;
                        var blendShapeTangents = blendShapeMesh.tangents;

                        var deltaVertices =
                            blendShapeVertices.Select((v, j) => v - vertices[j]).ToArray();
                        var deltaNormals =
                            blendShapeNormals.Length > 0
                            ? blendShapeNormals.Select((n, j) => n - normals[j]).ToArray()
                            : null;
                        var deltaTangents =
                            blendShapeTangents.Length > 0
                            ? blendShapeTangents.Select((t, j) => (Vector3)(t - tangents[j])).ToArray()
                            : null;

                        mesh.AddBlendShapeFrame(
                            blendShapeName,
                            blendShapeWeight,
                            deltaVertices,
                            deltaNormals,
                            deltaTangents
                            );
                    }
                    finally
                    {
                        GameObject.DestroyImmediate(blendShapeMesh);
                        smr.SetBlendShapeWeight(i, 0.0f);
                    }
                }
            }

            for (var i = 0; i < go.transform.childCount; ++i)
            {
                var ct = go.transform.GetChild(i);
                BakeMeshes(ct.gameObject);
            }
        }

        public static void NormalizeTransforms(Transform t, Matrix4x4 m)
        {
            // Apply parents' rotations and scales, then make local values of them to identity.
            var cm = m * Matrix4x4.TRS(Vector3.zero, t.localRotation, t.localScale);

            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            t.localPosition = m * t.localPosition;

            for (var i = 0; i < t.childCount; ++i)
            {
                var ct = t.GetChild(i);
                NormalizeTransforms(ct, cm);
            }
        }

        public void UpdateBonePoses(GameObject go)
        {
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                var bones = smr.bones;
                var newBindPoses = bones.Select(t =>
                {
                    // Ref: https://forum.unity.com/threads/runtime-model-import-wrong-bind-pose.276411/
                    var newTrans = t.worldToLocalMatrix * go.transform.localToWorldMatrix;
                    return newTrans;
                }).ToArray();
                Debug.Assert(newBindPoses.Count() == bones.Count());

                var mesh = bakedMeshes[smr.sharedMesh];
                mesh.bindposes = newBindPoses;
                mesh.RecalculateBounds();

                smr.sharedMesh = mesh;
            }

            for (var i = 0; i < go.transform.childCount; ++i)
            {
                var ct = go.transform.GetChild(i);
                UpdateBonePoses(ct.gameObject);
            }
        }
    }
}
