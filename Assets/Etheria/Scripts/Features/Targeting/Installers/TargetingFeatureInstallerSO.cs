using Etheria.Core.DI;
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

            builder.Register<TargetingService>(Lifetime.Singleton)
                .As<ITargetingService>();

            builder.Register<TargetSelectionState>(Lifetime.Singleton)
                .As<ITargetSelectionState>()
                .As<ITargetingEvents>();

            builder.RegisterEntryPoint<TargetingTracker>(Lifetime.Singleton);

            builder.Register<ControlledTargetProvider>(Lifetime.Singleton)
                .As<IControlledTargetProvider>();

            builder.Register<ViewRayProvider>(Lifetime.Singleton)
                .As<IViewRayProvider>();

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

            builder.Register<TargetAcquisitionService>(Lifetime.Singleton)
                .As<ITargetAcquisitionService>();

            builder.Register<TargetCycleService>(Lifetime.Singleton)
                .As<ITargetCycleService>();

            builder.Register<TargetCandidateProvider>(Lifetime.Singleton)
                .As<ITargetCandidateProvider>();

            builder.Register<TargetCandidateSnapshotProvider>(Lifetime.Singleton)
                .As<ITargetCandidateSnapshotProvider>();

            builder.Register<TargetValidator>(Lifetime.Singleton)
                .As<ITargetValidator>();

            builder.RegisterEntryPoint<TargetDebugMarkerPresenter>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<TargetDebugMarker>();
        }
    }
}

