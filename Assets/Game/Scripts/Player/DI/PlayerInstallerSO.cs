using Etheria.Core.DI;
using UnityEngine;
using VContainer;

namespace Game.Player
{
    [CreateAssetMenu(
        fileName = "PlayerInstaller",
        menuName = "Game/Player/Player Installer")]
    public sealed class PlayerInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<PlayerInputService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PlayerActorSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}