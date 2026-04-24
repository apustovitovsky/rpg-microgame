using System.Threading.Tasks;
using RPG.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "PickupInstaller", menuName = "RPG/Gameplay/Pickup/Pickup Installer")]
    public sealed class PickupInstallerSO : InstallerSO
    {
        [SerializeField] private PickupConfigSO _pickupConfig;

        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.RegisterInstance(_pickupConfig.PickupPrefab);

            builder.Register<PickupInteractionService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PickupFactory>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<PickupEntryPoint>(Lifetime.Singleton)
                .WithParameter(_pickupConfig.PickupDefinition);
        }
    }
}
