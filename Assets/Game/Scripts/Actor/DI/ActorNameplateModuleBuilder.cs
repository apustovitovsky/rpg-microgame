using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorNameplateModuleBuilder",
        menuName = "Game/Actor/Actor Nameplate Module Builder")]
    public sealed class ActorNameplateModuleBuilder : ModuleBuilder
    {
        [SerializeField] private ActorNameplateView _labelPrefab;

        public override void Install(IContainerBuilder builder)
        {
            if (_labelPrefab == null)
            {
                Debug.LogError(
                    $"{nameof(ActorNameplateModuleBuilder)} requires assigned actor label prefab.");

                return;
            }

            builder.RegisterComponentInHierarchy<ActorNameplatePoolHost>()
                .AsImplementedInterfaces();

            builder.RegisterInstance(_labelPrefab);

            builder.Register<ActorNameplatePool>(Lifetime.Singleton);
        }
    }
}