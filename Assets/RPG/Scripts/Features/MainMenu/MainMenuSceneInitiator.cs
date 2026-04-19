using System.Threading;
using Cysharp.Threading.Tasks;
using RPG.Core;
using VContainer.Unity;

namespace RPG.MainMenu
{
    public class MainMenuSceneInitiator : IAsyncStartable
    {
        private readonly SceneReadinessChannel _readinessChannel;

        public MainMenuSceneInitiator(SceneReadinessChannel readinessChannel)
        {
            _readinessChannel = readinessChannel;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
            _readinessChannel.Complete(true);
        }
    }
}
