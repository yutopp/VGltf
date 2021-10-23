using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VGltf.Unity
{
    public static class Utils
    {
        public static void Destroy(UnityEngine.Object o)
        {
            if (o == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(o);
            }
            else
#endif
            {
                UnityEngine.Object.Destroy(o);
            }
        }

        public struct DebugStopwatch : IDisposable
        {
            readonly System.Diagnostics.Stopwatch _stopwatch;
            readonly string _name;

            public DebugStopwatch(string name)
            {
                _name = name;
                _stopwatch = new System.Diagnostics.Stopwatch();

                _stopwatch.Start();
            }

            public void Dispose()
            {
                _stopwatch.Stop();

                float elapsed = (float)_stopwatch.Elapsed.TotalMilliseconds;
                Debug.Log($"Duration({_name}): {elapsed}ms");
            }
        }

        public static DebugStopwatch MeasureAndPrintTime(string name)
        {
            return new DebugStopwatch(name);
        }
    }
}
