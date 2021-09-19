using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VGltf;
using VGltf.Unity;

namespace VGltfExamples.VRMExample
{
    public sealed class Loader : MonoBehaviour
    {
        [SerializeField] InputField filePathInput;

        [SerializeField] public RuntimeAnimatorController RuntimeAnimatorController;

        sealed class VRMResource : IDisposable
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

        readonly List<VRMResource> _vrmResources = new List<VRMResource>();

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        void OnDestroy()
        {
            foreach (var disposable in _vrmResources)
            {
                disposable.Dispose();
            }
        }

        async UniTask<VRMResource> LoadVRM()
        {
            var filePath = filePathInput.text;

            // GLTFのコンテナを読む (unity非依存)
            var gltfContainer = await Task.Run(() =>
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    return GltfContainer.FromGlb(fs);
                }
            });

            var res = new VRMResource();
            try
            {
                // GLTFのsceneを指すGameObjectを作る。
                // 子のNodeのGameObjectがこの下に作成される。
                var go = new GameObject();
                res.Go = go;

                // VRM0はなぜかglTFの座標系の反転をZ軸で行うため
                var config = new Importer.Config
                {
                    FlipZAxisInsteadOfXAsix = true,
                };

                // GLTFのUnity向けImporterを作成
                // このImporterの内部のContextにリソースがキャッシュされる。
                // Importer(または内部のContext)をDisposeすることでリソースが解放される。
                using (var gltfImporter = new Importer(gltfContainer, config))
                {
                    var bridge = new VRM0ImporterBridge();
                    // VRM は glTF nodes にGameObjectがフラットに詰め込まれており、RootのGoが存在しないため hook で解消する
                    gltfImporter.AddHook(new VGltf.Ext.Vrm0.Unity.Hooks.ImporterHook(go, bridge));

                    // Sceneを読み込み
                    res.Context = gltfImporter.ImportSceneNodes();
                }
            }
            catch (Exception)
            {
                res.Dispose();
                throw;
            }

            return res;
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

            var res = await LoadVRM();
            _vrmResources.Insert(0, res);

            // 動かす
            var anim = res.Go.GetComponentInChildren<Animator>();
            anim.runtimeAnimatorController = RuntimeAnimatorController;

            var p1 = Common.MemoryProfile.Now;
            DebugLogProfile(p1, p0);
        }

        public void UIOnUnloadButtonClick()
        {
            if (_vrmResources.Count == 0)
            {
                return;
            }

            var p0 = Common.MemoryProfile.Now;
            DebugLogProfile(p0);

            var head = _vrmResources[0];
            _vrmResources.RemoveAt(0);

            head.Dispose();

            var p1 = Common.MemoryProfile.Now;
            DebugLogProfile(p1, p0);
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
    }
}
