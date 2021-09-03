using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VGltf.Unity
{
    public static class Utils
    {
        public static void Destroy(Object go)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                GameObject.DestroyImmediate(go);
            }
            else
#endif
            {
                GameObject.Destroy(go);
            }
        }
    }
}