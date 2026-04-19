using Cysharp.Threading.Tasks;

namespace RPG.Core
{
    public interface ISceneCoordinator
    {
        UniTask LoadMainMenuScene();
        UniTask LoadRPGScene();
        UniTask LoadFPSScene();
        UniTask LoadSyntyScene();
    }
}