using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VGltfExamples.Common
{
    // TimeSlicer is a utility class to slice the loading process of glTF.
    // If loading takes more than {limitMillisecPerFrame} ms to process on the main thread during {maxFrameCount} frames,
    // interrupt the loading and wait for the next frame to avoid continuing to block the main thread.
    public sealed class TimeSlicer : VGltf.Unity.ITimeSlicer
    {
        readonly int limitMillisecPerFrame = 16;
        readonly int maxFrameCount = 120;

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
