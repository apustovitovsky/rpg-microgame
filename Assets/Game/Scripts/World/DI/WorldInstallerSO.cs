using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.World
{
    [CreateAssetMenu(
        fileName = "WorldInstaller",
        menuName = "Game/World/World Installer")]
    public sealed class WorldInstallerSO : InstallerSO
    {
        [SerializeField] private WorldActorManifestSO _manifest;

        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_manifest);

            builder.RegisterEntryPoint<WorldActorLifecycleManager>(
                Lifetime.Singleton);
        }
    }
}