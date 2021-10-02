//
// Copyright (c) 2021- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;

namespace VGltf.UnitTests.Shims
{
    public sealed class ResourceLoaderFromAssets : IResourceLoader
    {
        readonly string _relBaseDir;

        public ResourceLoaderFromAssets(string relBaseDir)
        {
            _relBaseDir = relBaseDir;
        }

        public Resource Load(string uri)
        {
            if (DataUriUtil.IsData(uri))
            {
                return DataUriUtil.Extract(uri);
            }

            using (var fs = StreamReaderFactory.CreateStream(Path.Combine(_relBaseDir, uri)))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);

                var bytes = ms.ToArray();
                return new Resource
                {
                    Data = new ArraySegment<byte>(bytes),
                };
            }
        }

        public string FullPathOf(string uri)
        {
            throw new NotImplementedException();
        }
    }
}
