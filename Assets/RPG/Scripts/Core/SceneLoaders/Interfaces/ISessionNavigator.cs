using Cysharp.Threading.Tasks;

namespace RPG.Core
{
    public interface ISessionNavigator
    {
        UniTask LoadMainMenuScene();
        UniTask LoadRPGScene();
        UniTask LoadFPSScene();
        UniTask LoadSyntyScene();
    }
}
