using Etheria.Core.DI;
using Etheria.Game.Camera;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Game.DI
{
    [CreateAssetMenu(
        fileName = "GameInstaller",
        menuName = "Etheria/Game/Installers/Game Installer")]
    public class GameInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private GameSettingsSO _gameConfig;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.Register<InputSystem_Actions>(Lifetime.Singleton);

            builder.Register<GameTimeProvider>(Lifetime.Singleton)
                .As<IGameTimeProvider>();

            builder.Register<SceneStackLoadingService>(Lifetime.Singleton)
                .As<ISceneStackLoadingService>();

            builder.Register<GameNavigator>(Lifetime.Singleton)
                .As<IGameNavigator>();

            builder.RegisterEntryPoint<WorldStartup>(Lifetime.Singleton)
                .WithParameter(_gameConfig.SceneCatalog);
        }
    }
}
