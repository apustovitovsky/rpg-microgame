using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.Gameplay
{
    [CreateAssetMenu(fileName = "ActorInstaller", menuName = "RPG/Gameplay/Actor/Actor Installer")]
    public class ActorInstallerSO : InstallerSO
    {
        [SerializeField] private ActorConfigSO _actorConfig;

        public override void Install(in InstallContext context)
        {
            var builder = context.Builder;

            builder.RegisterInstance(_actorConfig);

            builder.RegisterComponentInHierarchy<ActorRuntimeRefs>()
                .UnderTransform(context.Scope.transform);

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
