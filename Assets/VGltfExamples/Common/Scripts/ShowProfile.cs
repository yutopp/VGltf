using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

namespace VGltfExamples.Common
{
    public sealed class MemoryProfile
    {
        public float TotalReservedMB;
        public float TotalAllocatedMB;
        public float TotalUnusedReservedMB;

        public static MemoryProfile Now
        {
            get
            {
                return new MemoryProfile
                {
                    TotalReservedMB = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f),
                    TotalAllocatedMB = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f),
                    TotalUnusedReservedMB = Profiler.GetTotalUnusedReservedMemoryLong() / (1024f * 1024f),
                };
            }
        }
    }

    public sealed class ShowProfile : MonoBehaviour
    {
        Text _text;

        void Start()
        {
            _text = gameObject.GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            var p = MemoryProfile.Now;
            _text.text = $"totalReservedMB = {p.TotalReservedMB}MB\n"
                + $"totalAllocatedMB = {p.TotalAllocatedMB}MB\n"
                + $"totalUnusedReservedMB = {p.TotalUnusedReservedMB}MB";
        }
    }
}
