//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace VGltf.UnitTests
{
    public class BufferBuilderTests
    {
        [Test]
        public void BuildTest()
        {
            var bufferBuilder = new BufferBuilder();

            var view0 = new ArraySegment<byte>(new byte[] { 0x00, 0x01, 0x02 });
            var view0Index = bufferBuilder.AddView(view0, 4, Types.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER);

            var view1 = new ArraySegment<byte>(new byte[] { 0x10, 0x11 });
            var view1Index = bufferBuilder.AddView(view1);

            var view2 = new ArraySegment<byte>(new byte[] { 0x20, 0x21, 0x22, 0x23 });
            var view2Index = bufferBuilder.AddView(view2);

            List<Types.BufferView> views;
            var bufferBytes = bufferBuilder.BuildBytes(out views);

            Assert.AreEqual(12, bufferBytes.Length); // Actual size is 5Bytes, but aligned to 4Bytes.
            Assert.That(bufferBytes, Is.EquivalentTo(new byte[] {
                        0x00, 0x01, 0x02, /* Align */ 0x00,
                        0x10, 0x11, /* Align */ 0x00, 0x00,
                        0x20, 0x21, 0x22, 0x23
                    }));

            Assert.AreEqual(3, views.Count);

            Assert.AreEqual(0, views[0].Buffer);
            Assert.AreEqual(0, views[0].ByteOffset);
            Assert.AreEqual(3, views[0].ByteLength);
            Assert.AreEqual(4, views[0].ByteStride);
            Assert.AreEqual(Types.BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER, views[0].Target);

            Assert.AreEqual(0, views[1].Buffer);
            Assert.AreEqual(4, views[1].ByteOffset);
            Assert.AreEqual(2, views[1].ByteLength);
            Assert.AreEqual(null, views[1].ByteStride);
            Assert.AreEqual(null, views[1].Target);

            Assert.AreEqual(0, views[2].Buffer);
            Assert.AreEqual(8, views[2].ByteOffset);
            Assert.AreEqual(4, views[2].ByteLength);
            Assert.AreEqual(null, views[2].ByteStride);
            Assert.AreEqual(null, views[2].Target);
        }
    }
}
