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
    public class TransformNormalizer : IDisposable
    {
        public GameObject Go;
        private Dictionary<Mesh, Mesh> bakedMeshes = new Dictionary<Mesh, Mesh>();

        public void Normalize(GameObject go)
        {
            Go = Normalized(go);
        }

        public GameObject Normalized(GameObject go)
        {
            var nGo = UnityEngine.Object.Instantiate(go) as GameObject;
            nGo.name = string.Format("{0}(VGltf.Normalized)", go.name);

            nGo.transform.localPosition = Vector3.zero;
            nGo.transform.localRotation = Quaternion.identity;
            nGo.transform.localScale = Vector3.one;

            BakeMeshes(nGo);
            NormalizeTransforms(nGo.transform, Matrix4x4.identity);
            UpdateBonePoses(nGo);

            return nGo;
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

                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                var mesh = new Mesh();
                smr.BakeMesh(mesh);

                mesh.name = string.Format("{0}(VGltf.Baked)", sharedMesh.name);
                mesh.boneWeights = weights;

                bakedMeshes.Add(sharedMesh, mesh);
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

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Go != null)
                    {
                        GameObject.DestroyImmediate(Go);
                    }

                    // Destroy baked meshes
                    foreach(var kv in bakedMeshes)
                    {
                        GameObject.Destroy(kv.Value);
                    }
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
