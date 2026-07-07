using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorNameplateConfigurator",
        menuName = "Game/Actor/Actor Nameplate Configurator")]
    public sealed class ActorNameplateConfiguratorSO : BuildConfigurator
    {
        [SerializeField] private ActorNameplateView _labelPrefab;

        public override void Install(IContainerBuilder builder)
        {
            if (_labelPrefab == null)
            {
                Debug.LogError(
                    $"{nameof(ActorNameplateConfiguratorSO)} requires assigned actor label prefab.");

                return;
            }

            builder.RegisterComponentInHierarchy<ActorNameplatePoolHost>()
                .AsImplementedInterfaces();

            builder.RegisterInstance(_labelPrefab);

            builder.Register<ActorNameplatePool>(Lifetime.Singleton);
        }
    }
}