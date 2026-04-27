using Cysharp.Threading.Tasks;

namespace Etheria.Core
{
    public interface ISessionNavigator
    {
        UniTask LoadMainMenuScene();
        UniTask LoadRPGScene();
        UniTask LoadFPSScene();
        UniTask LoadSyntyScene();
    }
}
