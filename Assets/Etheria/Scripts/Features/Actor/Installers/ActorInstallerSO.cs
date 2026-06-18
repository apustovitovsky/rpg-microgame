using Etheria.Core.DI;
using Etheria.Features.Targeting;
using Etheria.Game.Actor;
using Etheria.Game.Common;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Actor
{
    [CreateAssetMenu(
        fileName = "ActorInstaller",
        menuName = "Etheria/Gameplay/Actor/Actor Installer")]
    public class ActorInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {
            builder.RegisterComponentInHierarchy<ActorRuntimeRefs>()
                .UnderTransform(rootObject.transform)
                .AsSelf()
                .AsImplementedInterfaces();

            builder.Register<ActorInfo>(Lifetime.Singleton)
                .As<IPlayerAvatarInfo>();

            builder.Register<ActorTargetable>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PlayerAvatar>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterBuildCallback(container =>
            {
                rootObject.name = container.Resolve<IPlayerAvatarInfo>().DisplayName;
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
