using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features
{
    [CreateAssetMenu(
        fileName = "GameplayInstaller",
        menuName = "Etheria/Gameplay/Installers/Gameplay Installer")]
    public class GameplayInstallerSO : InstallerSO
    {
        [SerializeField] private GameplayConfigSO _gameplayConfig;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameplayConfig);
        }
    }
}

