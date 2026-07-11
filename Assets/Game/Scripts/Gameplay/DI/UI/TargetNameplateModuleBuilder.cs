using Game.Core;
using Game.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "TargetNameplateModuleBuilder",
        menuName = "Game/Gameplay/Target Nameplate Module Builder")]
    public sealed class TargetNameplateModuleBuilder : ModuleBuilder
    {
        [SerializeField] private TargetNameplateView _labelPrefab;

        public override void Install(IContainerBuilder builder)
        {
            if (_labelPrefab == null)
            {
                Debug.LogError(
                    $"{nameof(TargetNameplateModuleBuilder)} requires assigned target nameplate prefab.");

                return;
            }

            builder.RegisterComponentInHierarchy<TargetNameplatePoolHost>()
                .AsImplementedInterfaces();

            builder.RegisterInstance(_labelPrefab);

            builder.Register<TargetNameplatePool>(Lifetime.Singleton);
        }
    }
}