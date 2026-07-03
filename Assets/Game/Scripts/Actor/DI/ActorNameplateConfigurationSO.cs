using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorLabelUIInstaller",
        menuName = "Game/Actor/Actor Label UI Installer")]
    public sealed class ActorNameplateConfigurationSO : BuildConfigurationSO
    {
        [SerializeField] private ActorNameplateView _labelPrefab;

        public override void Install(IContainerBuilder builder)
        {
            if (_labelPrefab == null)
            {
                Debug.LogError(
                    $"{nameof(ActorNameplateConfigurationSO)} requires assigned actor label prefab.");

                return;
            }

            builder.RegisterComponentInHierarchy<ActorNameplatePoolHost>()
                .AsImplementedInterfaces();

            builder.RegisterInstance(_labelPrefab);

            builder.Register<ActorNameplatePool>(Lifetime.Singleton);
        }
    }
}