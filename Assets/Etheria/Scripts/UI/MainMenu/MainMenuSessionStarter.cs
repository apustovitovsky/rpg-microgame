using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Etheria.MainMenu
{
    public class MainMenuSessionStarter : IAsyncStartable
    {
        public UniTask StartAsync(CancellationToken cancellation = default)
        {
            return UniTask.CompletedTask;
        }
    }
}
