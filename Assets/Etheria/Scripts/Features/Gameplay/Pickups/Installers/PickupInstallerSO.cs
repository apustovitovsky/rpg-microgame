using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Gameplay
{
    [CreateAssetMenu(fileName = "PickupInstaller", menuName = "Etheria/Gameplay/Pickup/Pickup Installer")]
    public sealed class PickupInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private PickupConfigSO _pickupConfig;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterInstance(_pickupConfig.PickupPrefab).As<Pickup>();
            builder.RegisterInstance(_pickupConfig.PickupDefinition);

            builder.Register<PickupInteractionHandler>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<PickupEntryPoint>();
        }
    }
}

