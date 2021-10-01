//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace VGltf.Unity.UnitTests
{
    public class IdentityTests
    {
        // Combining UnityTest, TestCaseSource and IEnumerator doesn't work...
        [UnityTest]
        //[TestCaseSource(nameof(PrimitiveTypes))]
        public IEnumerator IdentityTestRunner(/*PrimitiveType primType*/)
        {
            foreach(var v in PrimitiveTypes)
            {
                var task = IdentityTest((PrimitiveType) ((object[]) v)[0]);
                yield return task.AsIEnumerator();
            }
        }

        async Task IdentityTest(PrimitiveType primType)
        {
            GameObject srcGo = GameObject.CreatePrimitive(primType);

            // Serialize
            byte[] glb;
            using (var exporter = new Exporter())
            using (var s = new MemoryStream())
            {
                exporter.ExportGameObjectAsScene(srcGo);
                var gltfContainer = exporter.IntoGlbContainer();

                Glb.Writer.WriteFromContainer(s, gltfContainer);
                s.Flush();

                glb = s.ToArray();
            }

            // Deserialize
            GltfContainer gltfConteiner;
            using (var s = new MemoryStream(glb))
            {
                gltfConteiner = Glb.Reader.ReadAsContainer(s);
            }
            var scene = VGltf.Types.Extensions.GltfExtensions.GetSceneObject(gltfConteiner.Gltf);
            Assert.That(scene.Nodes.Count, Is.EqualTo(1));

            var rootNodeIndex = scene.Nodes[0];

            IImporterContext ctx = null;
            using (var importer = new Importer(gltfConteiner, new DefaultTimeSlicer()))
            {
                ctx = await importer.ImportSceneNodes(System.Threading.CancellationToken.None);
            }
            using (ctx)
            {
                var rootGo = ctx.Resources.Nodes[rootNodeIndex].Value.gameObject;

                // Tests
                var srcMf = srcGo.GetComponent<MeshFilter>();
                var srcMesh = srcMf.sharedMesh;

                var dstGo = rootGo;
                var dstMf = dstGo.GetComponent<MeshFilter>();
                var dstMesh = dstMf.sharedMesh;

                Assert.That(srcMesh.vertices, Is.EquivalentTo(dstMesh.vertices).Using<Vector3, Vector3>(EqualsWithDelta));
                Assert.That(srcMesh.normals, Is.EquivalentTo(dstMesh.normals).Using<Vector3, Vector3>(EqualsWithDelta));
                Assert.That(srcMesh.uv, Is.EquivalentTo(dstMesh.uv).Using<Vector2, Vector2>(EqualsWithDelta));
                Assert.That(srcMesh.uv2, Is.EquivalentTo(dstMesh.uv2).Using<Vector2, Vector2>(EqualsWithDelta));
                Assert.That(srcMesh.colors, Is.EquivalentTo(dstMesh.colors));
                Assert.That(srcMesh.boneWeights, Is.EquivalentTo(dstMesh.boneWeights));
            }
        }

        static bool EqualsWithDelta(Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b) < 0.001f;
        }

        static bool EqualsWithDelta(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) < 0.001f;
        }

        public static object[] PrimitiveTypes = {
            new object[] {
                PrimitiveType.Cube
            },
            new object[] {
                PrimitiveType.Plane,
            },
            new object[] {
                PrimitiveType.Sphere,
            },
        };

        //
        //
        //

        void Trace(GameObject a, GameObject b)
        {
            Debug.Log($"Checking => {a.name} == {b.name}");
            Assert.IsTrue(Asserts.EqualsWithDelta(a.transform, b.transform));

            var asmr = a.GetComponent<SkinnedMeshRenderer>();
            var bsmr = b.GetComponent<SkinnedMeshRenderer>();
            if (asmr != null && bsmr != null)
            {
                var am = asmr.sharedMesh;
                var bm = bsmr.sharedMesh;

                Asserts.AssertEqualsWithDelta(am.vertices, bm.vertices);
                Asserts.AssertEqualsWithDelta(am.normals, bm.normals);
                Asserts.AssertEqualsWithDelta(am.boneWeights, bm.boneWeights);
                Asserts.AssertEqualsWithDelta(am.bindposes, bm.bindposes);

                Assert.IsTrue(Asserts.EqualsWithDelta(asmr.rootBone, bsmr.rootBone));
                Asserts.AssertEqualsWithDelta(asmr.bones, bsmr.bones);

            }
            else if (asmr != null || bsmr != null)
            {
                throw new NotImplementedException();
            }

            Assert.Equals(a.transform.childCount, b.transform.childCount);

            for (int i = 0; i < a.transform.childCount; ++i)
            {
                var ac = a.transform.GetChild(i);
                var bc = b.transform.GetChild(i);
                Trace(ac.gameObject, bc.gameObject);
            }
        }
    }
}
