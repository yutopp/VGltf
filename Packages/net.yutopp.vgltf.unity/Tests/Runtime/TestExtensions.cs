using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace VGltf.Unity.UnitTests
{
    public static class TestExtensions
    {
        public static IEnumerator AsIEnumerator(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
            }

            yield return null;
        }
    }
}
