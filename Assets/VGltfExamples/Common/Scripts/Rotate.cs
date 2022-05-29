using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VGltfExamples.Common
{
    public sealed class Rotate : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            gameObject.transform.Rotate(new Vector3(0, 1, 0));
        }
    }
}
