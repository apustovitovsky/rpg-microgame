using Etheria.Core.DI;
using Etheria.Game.Player;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Targeting
{
    [CreateAssetMenu(
        fileName = "TargetSystemInstaller",
        menuName = "Etheria/Gameplay/Targeting/Target System Installer")]
    public class TargetingFeatureInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private TargetingSettingsSO _targetingSettings;

        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterInstance(_targetingSettings);

            builder.RegisterEntryPoint<TargetingTracker>(Lifetime.Singleton);

            builder.Register<ControlledTargetProvider>(Lifetime.Singleton)
                .As<IControlledTargetProvider>();

            builder.Register<ViewRayProvider>(Lifetime.Singleton)
                .As<IViewRayProvider>();

            builder.Register<TargetingService>(Lifetime.Singleton)
                .As<ITargetingService>();

            builder.Register<SphereCastTargetHitProvider>(Lifetime.Singleton)
                .As<ITargetHitProvider>();

            builder.Register<TargetCandidateResolver>(Lifetime.Singleton)
                .As<ITargetCandidateResolver>();

            builder.Register<ViewConeTargetCandidateFilter>(Lifetime.Singleton)
                .As<ITargetCandidateFilter>();

            builder.Register<TargetLineOfSightChecker>(Lifetime.Singleton)
                .As<ITargetLineOfSightChecker>();

            builder.Register<TargetCandidateEvaluator>(Lifetime.Singleton)
                .As<ITargetCandidateEvaluator>();

            builder.Register<TargetCandidateComparer>(Lifetime.Singleton)
                .As<ITargetCandidateComparer>();

            builder.Register<TargetCandidateSelector>(Lifetime.Singleton)
                .As<ITargetCandidateSelector>();

            builder.Register<TargetCandidateProvider>(Lifetime.Singleton)
                .As<ITargetCandidateProvider>();

            builder.RegisterEntryPoint<TargetDebugMarkerPresenter>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<TargetDebugMarker>();
        }
    }
}

