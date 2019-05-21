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

namespace VGltf.Unity.UnitTests.ModelTester
{
    using VGltf.UnitTests;
    using VJson.Schema;

    public class BoxTexturedImportingTester : VGltf.UnitTests.ModelTester.IModelTester
    {
        public async void TestModel(ResourcesStore store)
        {
            var res = new ResourcesOnMemory();
            var importer = new Importer(res, store);

            await importer.ImportAsync();
        }
    }
}
