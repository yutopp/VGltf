using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VGltf;
using VGltf.Unity;

namespace VGltfExamples.Dynamic
{
    public class Loader : MonoBehaviour
    {
        [SerializeField] public string FilePath = "SampleModels/Alicia/VRM/AliciaSolid.vrm";

        void Start()
        {
            GltfContainer gltfContainer;
            using (var fs = new FileStream(FilePath, FileMode.Open))
            {
                gltfContainer = GltfContainer.FromGlb(fs);
            }
            
            var gltfImporter = new Importer(gltfContainer);

            var go = new GameObject();
            gltfImporter.ImportSceneNodes(go);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
