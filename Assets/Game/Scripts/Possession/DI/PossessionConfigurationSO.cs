using Game.Core;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Possession
{
    [CreateAssetMenu(
        fileName = "PossessionConfiguration",
        menuName = "Game/Possession/Possession Configuration")]
    public sealed class PossessionConfigurationSO : BuildConfigurationSO
    {
        [SerializeField] private CinemachineCamera _virtualCameraPrefab;
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<PossessionService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<PossessedActorTargetLabelPresenter>(
                Lifetime.Singleton);

            builder.RegisterComponentInNewPrefab(
                _virtualCameraPrefab,
                Lifetime.Singleton);
        }
    }
}