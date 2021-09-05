using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VGltf;
using VGltf.Unity;

namespace VGltfExamples.Dynamic
{
    public class Loader : MonoBehaviour
    {
        [SerializeField] public string FilePath = "SampleModels/Alicia/VRM/AliciaSolid.vrm";

        [SerializeField] public RuntimeAnimatorController RuntimeAnimatorController;

        class VRMResource : IDisposable
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

        List<VRMResource> _vrmResources = new List<VRMResource>();

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
            // GLTFのコンテナを読む (unity非依存)
            GltfContainer gltfContainer;
            using (var fs = new FileStream(FilePath, FileMode.Open))
            {
                gltfContainer = GltfContainer.FromGlb(fs);
            }

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
                    // VRM は glTF nodes にGameObjectがフラットに詰め込まれており、RootのGoが存在しないため hook で解消する
                    gltfImporter.AddHook(new VGltf.Ext.Vrm0.Unity.Hooks.ImporterHook(go));

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
            var p0 = Ext.MemoryProfile.Now;
            DebugLogProfile(p0);

            var res = await LoadVRM();
            _vrmResources.Insert(0, res);

            // 動かす
            var anim = res.Go.GetComponentInChildren<Animator>();
            anim.runtimeAnimatorController = RuntimeAnimatorController;

            var p1 = Ext.MemoryProfile.Now;
            DebugLogProfile(p1, p0);
        }

        public void UIOnUnloadButtonClick()
        {
            if (_vrmResources.Count == 0)
            {
                return;
            }

            var p0 = Ext.MemoryProfile.Now;
            DebugLogProfile(p0);

            var head = _vrmResources[0];
            _vrmResources.RemoveAt(0);

            head.Dispose();

            var p1 = Ext.MemoryProfile.Now;
            DebugLogProfile(p1, p0);
        }

        void DebugLogProfile(Ext.MemoryProfile now, Ext.MemoryProfile prev = null)
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
