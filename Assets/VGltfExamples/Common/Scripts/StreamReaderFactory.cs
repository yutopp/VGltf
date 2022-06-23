using System.IO;
using UnityEngine;
# if !UNITY_EDITOR && UNITY_ANDROID
using System;
using UnityEngine.Networking;
# endif

namespace VGltfExamples.Common
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
