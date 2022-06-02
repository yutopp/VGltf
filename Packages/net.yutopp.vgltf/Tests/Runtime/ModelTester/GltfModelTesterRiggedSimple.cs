//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using NUnit.Framework;
using VGltf.UnitTests.Shims;

namespace VGltf.UnitTests.ModelTester
{
    using Vec4UShort = Vec4<ushort>;
    using Vec4Float = Vec4<float>;

    // See: https://github.com/KhronosGroup/glTF-Sample-Models/tree/master/2.0/RiggedSimple
    public class RiggedSimpleTester : IModelTester
    {
        public void TestModel(ResourcesStore store)
        {
            var gltf = store.Gltf;

            var mesh = gltf.Meshes[0];
            var primitive = mesh.Primitives[0];
            Assert.AreEqual(1, primitive.Attributes["JOINTS_0"]);
            Assert.AreEqual(4, primitive.Attributes["WEIGHTS_0"]);

            // Index 1
            var joints0 = store.GetOrLoadTypedBufferByAccessorIndex(primitive.Attributes["JOINTS_0"]);
            Assert.AreEqual(96, joints0.GetEntity<ushort, Vec4UShort>(Vec4UShort.FromArray).AsArray().Length);

            // Index 4
            var weights0 = store.GetOrLoadTypedBufferByAccessorIndex(primitive.Attributes["WEIGHTS_0"]);
            Assert.AreEqual(96, weights0.GetEntity<float, Vec4Float>(Vec4Float.FromArray).AsArray().Length);
        }
    }
}
