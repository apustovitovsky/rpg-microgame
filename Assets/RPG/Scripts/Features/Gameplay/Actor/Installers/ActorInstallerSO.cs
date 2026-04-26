using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "ActorInstaller", menuName = "RPG/Gameplay/Actor/Actor Installer")]
    public class ActorInstallerSO : ScopeInstallerSO
    {

        public override void Install(LifetimeScope scope, IContainerBuilder builder)
        {

            builder.RegisterComponentInHierarchy<ActorRuntimeRefs>()
                .UnderTransform(scope.transform);

            builder.Register<ActorTargetable>(Lifetime.Singleton)
                .WithParameter(scope.gameObject.name)
                .As<ITargetable>();

            builder.RegisterBuildCallback(container =>
            {
                scope.name = container.Resolve<IActorNamingService>().Generate();
                var runtimeRefs = container.Resolve<ActorRuntimeRefs>();
                var targetable = container.Resolve<ITargetable>();

                if (runtimeRefs.Hitboxes == null)
                    return;

                foreach (var hitbox in runtimeRefs.Hitboxes)
                {
                    if (hitbox == null)
                        continue;

                    hitbox.Initialize(targetable);
                }
            });
        }
    }
}
