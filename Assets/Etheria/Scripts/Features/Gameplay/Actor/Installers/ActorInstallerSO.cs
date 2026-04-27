using Etheria.Core.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Gameplay
{
    [CreateAssetMenu(fileName = "ActorInstaller", menuName = "Etheria/Gameplay/Actor/Actor Installer")]
    public class ActorInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {

            builder.RegisterComponentInHierarchy<ActorRuntimeRefs>()
                .UnderTransform(rootObject.transform);

            builder.Register<ActorTargetable>(Lifetime.Singleton)
                .WithParameter(rootObject.name)
                .As<ITargetable>();

            builder.RegisterBuildCallback(container =>
            {
                rootObject.name = container.Resolve<IActorNamingService>().Generate();
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
