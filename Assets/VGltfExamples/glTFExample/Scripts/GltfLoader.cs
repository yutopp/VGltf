using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VGltf;
using VGltf.Unity;

namespace VGltfExamples.GltfExamples
{
    public sealed class GltfLoader : MonoBehaviour
    {
        [SerializeField] Dropdown filePathInput;

        [SerializeField] Button loadButton;
        [SerializeField] Button unloadButton;
        [SerializeField] Button exportButton;

        sealed class GltfResource : IDisposable
        {
            public IImporterContext Context;
            public GameObject Go;

            public void Dispose()
            {
                if (Go != null)
                {
                    GameObject.Destroy(Go);
                }
                Context?.Dispose();
            }
        }

        readonly List<GltfResource> _modelResources = new List<GltfResource>();

        readonly List<string[]> _modelLocs = new List<string[]>
        {
            new string[]{"TextureLinearInterpolationTest", "glTF-Binary", "TextureLinearInterpolationTest.glb"},
            new string[]{"TextureEncodingTest", "glTF-Binary", "TextureEncodingTest.glb"},
            new string[]{"NormalTangentTest", "glTF-Binary", "NormalTangentTest.glb"},
            new string[]{"NormalTangentMirrorTest", "glTF-Binary", "NormalTangentMirrorTest.glb"},
            new string[]{"MetalRoughSpheres", "glTF-Binary", "MetalRoughSpheres.glb"},
            new string[]{"AlphaBlendModeTest", "glTF-Binary", "AlphaBlendModeTest.glb"},
            new string[]{"EmissiveStrengthTest", "glTF-Binary", "EmissiveStrengthTest.glb"},
            new string[]{"WaterBottle", "glTF-Binary", "WaterBottle.glb"},
            new string[]{"BoomBox", "glTF-Binary", "BoomBox.glb"},
            new string[]{"NormalTest02", "normal_test_02.glb"},
        };

        void Start()
        {
            filePathInput.AddOptions(_modelLocs.Select(xs => new Dropdown.OptionData(xs.Last())).ToList());

            loadButton.onClick.AddListener(UIOnLoadButtonClick);
            unloadButton.onClick.AddListener(UIOnUnloadButtonClick);
            exportButton.onClick.AddListener(UIOnExportButtonClick);
        }

        // Update is called once per frame
        void Update()
        {
        }

        void OnDestroy()
        {
            foreach (var disposable in _modelResources)
            {
                disposable.Dispose();
            }
        }

        async UniTask<GltfResource> LoadGltf(string filePath, string name, System.Threading.CancellationToken ct)
        {
            // Read the glTF container.
            // Task.Run is used to avoid blocking the main thread.
            // GltfContainer is not dependent on Unity, so it can be used in other threads not only in the main thread.
            var gltfContainer = await Task.Run(() =>
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    return GltfContainer.FromGlb(fs);
                }
            }, ct);

            // GltfResource is a wrapper of the resources only used in examples.
            // It is used for avoid resource leaks when an exception occurs.
            var res = new GltfResource();
            try
            {
                // Create a GameObject that points to root nodes in the glTF scene.
                // GameObjects of the glTF's child node will be created under the this object.
                var go = new GameObject();
                go.name = name;

                res.Go = go;

                // Create a TimeSlicer for Unity.
                // TimeSlicer is used to control the time spent in the main thread.
                var timeSlicer = new Common.TimeSlicer();

                // Create a glTF Importer for Unity.
                // The resources will be stored in the Context in this Importer.
                // Resources can be released by calling Dispose of the Importer (or the Context).
                using (var gltfImporter = new Importer(gltfContainer, timeSlicer))
                {
                    // Load the Scene.
                    // ImportSceneNodes moves ownership of the Context from the Importer, so resources will not be released when the Importer is disposed.
                    // The Context must be disposed by the caller.
                    res.Context = await gltfImporter.ImportSceneNodes(ct);
                }

                foreach (var rootNodeIndex in gltfContainer.Gltf.RootNodesIndices)
                {
                    var rootNode = res.Context.Resources.Nodes[rootNodeIndex];
                    rootNode.Value.transform.SetParent(go.transform, false);
                }
            }
            catch (Exception)
            {
                res.Dispose();
                throw;
            }

            return res;
        }

        UniTask ExportGltf(string filePath, GameObject go)
        {
            try
            {
                // Write the glTF container (unity-independent)
                var gltfContainer = default(GltfContainer);

                using (var gltfExporter = new Exporter(new Exporter.Config
                {
                    IncludeRootObject = false,
                }))
                {
                    gltfExporter.ExportGameObjectAsScene(go);

                    gltfContainer = gltfExporter.IntoGlbContainer();
                }

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    GltfContainer.ToGlb(fs, gltfContainer);
                }

                Debug.Log($"Exported!: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return UniTask.CompletedTask;
        }

        // UI

        public void UIOnLoadButtonClick()
        {
            UIOnLoadButtonClickAsync().Forget();
        }

        public async UniTaskVoid UIOnLoadButtonClickAsync()
        {
            var p0 = Common.MemoryProfile.Now;
            DebugLogProfile(p0);

            var loc = _modelLocs[filePathInput.value];
            var filePath = SampleAssetPath(loc); // TODO: Support Android

            var res = await LoadGltf(filePath, "glTF", System.Threading.CancellationToken.None);
            _modelResources.Insert(0, res);

            var p1 = Common.MemoryProfile.Now;
            DebugLogProfile(p1, p0);
        }

        public void UIOnUnloadButtonClick()
        {
            if (_modelResources.Count == 0)
            {
                return;
            }

            var p0 = Common.MemoryProfile.Now;
            DebugLogProfile(p0);

            var head = _modelResources[0];
            _modelResources.RemoveAt(0);

            head.Dispose();

            var p1 = Common.MemoryProfile.Now;
            DebugLogProfile(p1, p0);
        }

        public void UIOnExportButtonClick()
        {
            if (_modelResources.Count == 0)
            {
                return;
            }

            var res = _modelResources[0];
            ExportGltf("out.glb", res.Go).Forget();
        }

        void DebugLogProfile(Common.MemoryProfile now, Common.MemoryProfile prev = null)
        {
            Debug.Log($"----------");
            Debug.Log($"(totalReservedMB, totalAllocatedMB, totalUnusedReservedMB)");
            Debug.Log($"({now.TotalReservedMB}MB,  {now.TotalAllocatedMB}MB, {now.TotalUnusedReservedMB}MB");

            if (prev != null)
            {
                Debug.Log($"delta ({now.TotalReservedMB - prev.TotalReservedMB}MB, {now.TotalAllocatedMB - prev.TotalAllocatedMB}MB, {now.TotalUnusedReservedMB - prev.TotalUnusedReservedMB}MB)");
            }
        }

        static string SampleAssetPath(params string[] names)
        {
            var a = new List<string> { Application.streamingAssetsPath, "SampleModels" };
            a.AddRange(names);

            return Path.Combine(a.ToArray());
        }
    }
}
