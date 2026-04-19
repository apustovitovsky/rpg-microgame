using System.Threading;
using Cysharp.Threading.Tasks;
using RPG.Core;
using VContainer.Unity;

namespace RPG.MainMenu
{
    public class MainMenuSceneInitiator : ISceneInitiator, IAsyncStartable
    {
        private readonly UniTaskCompletionSource<bool> _readyTcs = new();
        public UniTask<bool> Ready => _readyTcs.Task;

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
            _readyTcs.TrySetResult(true);
        }
    }
}
