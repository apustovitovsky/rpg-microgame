using RPG.Core;
using UnityEngine;
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

            builder.Register<PickupCollectionService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterFactory<PickupDefinitionSO, IPickupInstance>(
                (definition) => new PickupInstance(definition));
        }
    }
}
