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
            new string[]{"BoomBox", "glTF-Binary", "BoomBox.glb"},
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

        async UniTask<GltfResource> LoadGltf(string filePath, string name)
        {
            // Read the glTF container (unity-independent)
            var gltfContainer = await Task.Run(() =>
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    return GltfContainer.FromGlb(fs);
                }
            });

            var res = new GltfResource();
            try
            {
                // Create a GameObject that points to root nodes in the glTF scene.
                // The GameObject of the glTF's child Node will be created under this object.
                var go = new GameObject();
                go.name = name;

                res.Go = go;

                // Create a glTF Importer for Unity.
                // The resources will be cached in the internal Context of this Importer.
                // Resources can be released by calling Dispose of the Importer (or the internal Context).
                var timeSlicer = new Common.TimeSlicer();
                using (var gltfImporter = new Importer(gltfContainer, timeSlicer))
                {
                    // Load the Scene.
                    res.Context = await gltfImporter.ImportSceneNodes(System.Threading.CancellationToken.None);
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

            var res = await LoadGltf(filePath, "glTF");
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
