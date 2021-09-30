using System.Collections;
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
                throw task.Exception;
            }
        }
    }
}
