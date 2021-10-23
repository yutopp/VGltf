using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VGltfExamples.Common
{
    public sealed class TimeSlicer : VGltf.Unity.ITimeSlicer
    {
        readonly int limitMillisecPerFrame = 20;
        readonly int maxFrameCount = 30; // 30フレーム以内に読まれれば良い

        readonly System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        int elapsedFrame = 0;

        public TimeSlicer()
        {
            sw.Start();
        }

        public async Task Slice(CancellationToken ct)
        {
            // Debug.Log($"Slice check: {sw.ElapsedMilliseconds} >= {limitMillisecPerFrame}");
            if (sw.ElapsedMilliseconds >= limitMillisecPerFrame)
            {
                Debug.Log("Slice!");

                await UniTask.DelayFrame(1, cancellationToken: ct);
                elapsedFrame++;
                sw.Stop();
                sw.Reset();

                if (elapsedFrame < maxFrameCount)
                {
                    sw.Start();
                }
            }
        }
    }
}
