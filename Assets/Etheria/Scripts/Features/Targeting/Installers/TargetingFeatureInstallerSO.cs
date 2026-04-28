using Etheria.Core.DI;
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

            builder.Register<ColliderTargetResolver>(Lifetime.Singleton);

            builder.RegisterEntryPoint<TargetingTracker>(Lifetime.Singleton);


            builder.Register<CameraRayProvider>(Lifetime.Singleton)
                .As<ICameraRayProvider>();

            builder.Register<CameraRayTargetDetectionService>(Lifetime.Singleton)
                .As<ITargetDetectionService>();

            builder.Register<TargetingService>(Lifetime.Singleton)
                .As<ITargetingService>();

            builder.Register<TargetingController>(Lifetime.Singleton);

            builder.RegisterEntryPoint<TargetDebugMarkerPresenter>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<TargetDebugMarker>();
        }
    }
}

