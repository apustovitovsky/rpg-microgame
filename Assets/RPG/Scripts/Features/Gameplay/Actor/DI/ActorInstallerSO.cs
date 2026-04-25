using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "ActorInstaller", menuName = "RPG/Gameplay/Actor/Actor Installer")]
    public class ActorInstallerSO : ScopeInstallerSO
    {
        [SerializeField] private ActorConfigSO _actorConfig;

        public override void Install(LifetimeScope scope, IContainerBuilder builder)
        {
            builder.RegisterInstance(_actorConfig);

            builder.RegisterComponentInHierarchy<ActorRuntimeRefs>()
                .UnderTransform(scope.transform);

            builder.Register<ActorTargetable>(Lifetime.Scoped)
                .As<IActorTargetable>();

            builder.RegisterBuildCallback(container =>
            {
                var runtimeRefs = container.Resolve<ActorRuntimeRefs>();
                var targetable = container.Resolve<IActorTargetable>();

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
