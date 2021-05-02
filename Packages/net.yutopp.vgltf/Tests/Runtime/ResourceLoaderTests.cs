//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using NUnit.Framework;

namespace VGltf.UnitTests
{
    public class ResourceLoaderFromFileStorageTests
    {
        [Test]
        public void ValidPathTest()
        {
            Assert.That(ResourceLoaderFromFileStorage.EnsureCleanedPath("/tmp/aaa", "bbb.json"),
                        Is.EqualTo("/tmp/aaa/bbb.json"));
        }

        [Test]
        public void InvalidPathTest()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                ResourceLoaderFromFileStorage.EnsureCleanedPath("/tmp/aaa", "../bbb.json");
            });
            Assert.That(ex.Message, Is.EqualTo("Path must be a child of baseDir: Uri = ../bbb.json, BaseDir = /tmp/aaa, FullPath = /tmp/bbb.json"));
        }
    }
}
