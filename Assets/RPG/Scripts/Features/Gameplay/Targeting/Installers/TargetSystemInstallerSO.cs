using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(
        fileName = "TargetSystemInstaller",
        menuName = "RPG/Gameplay/Targeting/Target System Installer")]
    public class TargetSystemInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private TargetingSettingsSO _targetingSettings;

        public override void Install(LifetimeScope scope, IContainerBuilder builder)
        {
            builder.RegisterInstance(_targetingSettings);
            builder.RegisterComponentInHierarchy<Camera>();

            builder.Register<ColliderTargetResolver>(Lifetime.Singleton);

            builder.RegisterEntryPoint<TargetingTracker>(Lifetime.Singleton);

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
