using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Gameplay
{
    [CreateAssetMenu(fileName = "ActorPickupServicesInstaller", menuName = "Etheria/Gameplay/Pickup/Actor Pickup Services Installer")]
    public sealed class ActorPickupServicesInstallerSO : ScopeInstallerSO
    {
        [Header("Parameters")]

        [Tooltip("Amount of actor health")]
        [SerializeField] private float curHealth = 25f;
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Amount of actor gold")]
        [SerializeField] private int curGold = 5;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.Register<ActorHealth>(Lifetime.Scoped)
                .WithParameter(nameof(curHealth), curHealth)
                .WithParameter(nameof(maxHealth), maxHealth)
                .AsImplementedInterfaces();

            builder.Register<ActorInventory>(Lifetime.Scoped)
                .WithParameter(curGold)
                .AsImplementedInterfaces();

            builder.RegisterComponentInHierarchy<PickupCollector>()
                .UnderTransform(rootObject.transform);
        }
    }
}
