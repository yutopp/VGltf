//
// Copyright (c) 2021- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;

namespace VGltf.UnitTests.Shims
{
    public static class StreamReaderFactory
    {
        public static Stream CreateStream(string path)
        {
#if UNITY_5_3_OR_NEWER
            return UnityStreamingAssets.StreamReaderFactory.Create(path);
#else
            return File.OpenRead(Path.Combine("Assets", Path.Combine("StreamingAssets", path)));
#endif
        }
    }
}
