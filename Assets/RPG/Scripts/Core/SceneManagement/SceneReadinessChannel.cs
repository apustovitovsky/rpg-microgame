using Cysharp.Threading.Tasks;

namespace RPG.Core
{
    public sealed class SceneReadinessChannel
    {
        private UniTaskCompletionSource<bool> _tcs = new();

        public UniTask<bool> Completion => _tcs.Task;

        public void Complete(bool result) => _tcs.TrySetResult(result);

        public void Reset() => _tcs = new UniTaskCompletionSource<bool>();
    }
}
