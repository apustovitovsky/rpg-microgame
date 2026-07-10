using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
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
                    $"{nameof(TargetNameplateModuleBuilder)} requires assigned actor label prefab.");

                return;
            }

            builder.RegisterComponentInHierarchy<TargetNameplatePoolHost>()
                .AsImplementedInterfaces();

            builder.RegisterInstance(_labelPrefab);

            builder.Register<TargetNameplatePool>(Lifetime.Singleton);
        }
    }
}