//
// Copyright (c) 2021- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

//
// NOTE:
//  Since VGltf by itself is a standalone C# library, I don't want to put Unity-related code in this file originally.
//  However, in order to run tests in pure C# and Unity (especially Android), we have to share the same implementation through StreamingAssets.
//  Therefore, we will make a special exception and use Unity's functions for file loading only when there are Unity-related macros.
//  Also, make sure that no Unity-related code is used in pure C#.
//
#if UNITY_5_3_OR_NEWER
# if !UNITY_EDITOR && UNITY_ANDROID
using System;
using UnityEngine.Networking;
# endif
using System.IO;
using UnityEngine;

namespace VGltf.UnitTests.Shims.UnityStreamingAssets
{
    public static class StreamReaderFactory
    {
# if !UNITY_EDITOR && UNITY_ANDROID
        public static Stream Create(string path)
        {
            var basePath = "jar:file://" + Application.dataPath + "!/assets/";
            var url = basePath + path;
            using (var req = UnityWebRequest.Get(url))
            {
                req.SendWebRequest();

                while (!req.isDone && !req.isNetworkError && !req.isHttpError) { }
                if (req.isNetworkError || req.isHttpError) {
                    throw new Exception($"Failed to load: url = {url}, error = {req.error}");
                }

                return new MemoryStream(req.downloadHandler.data);
            }
        }
# elif !UNITY_EDITOR && UNITY_IOS
        public static Stream Create(string path)
        {
            var basePath = Path.Combine(Application.dataPath, "Raw");
            return File.OpenRead(Path.Combine(basePath, path));
        }
# else
        public static Stream Create(string path)
        {
            var basePath = Path.Combine(Application.dataPath, "StreamingAssets");
            return File.OpenRead(Path.Combine(basePath, path));
        }
# endif
    }
}
#endif
