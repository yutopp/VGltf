//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace VGltf.UnitTests
{
    public class GLBReaderTests
    {
        [Test]
        [TestCaseSource("ValuesArgs")]
        public void ReaderTest(string[] modelPath, object expect)
        {
            var path = modelPath.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var c = GltfContainer.FromGlb(fs);
            }
        }

        public static object[] ValuesArgs = {
            new object[] {
                new string[] {"Alicia","VRM","AliciaSolid.vrm"},
                null,
            }
        };
    }
}
