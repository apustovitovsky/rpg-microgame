using Etheria.Core.DI;
using Etheria.Game.Targeting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Features.Character
{
    [CreateAssetMenu(
        fileName = "CharacterInstaller",
        menuName = "Etheria/Features/Character/Character Installer")]
    public class CharacterInstallerSO : ScopeInstallerSO
    {
        public override void Install(IContainerBuilder builder, GameObject rootObject)
        {

            builder.RegisterEntryPoint<CharacterInteractionService>(
                Lifetime.Singleton);

            var cameraLookController =
                rootObject.GetComponentInChildren<PlayerCameraLookController>(true);

            var characterController =
                rootObject.GetComponentInChildren<PlayerCharacterAnimationController>(true);

            var targetSensor =
                rootObject.GetComponentInChildren<CharacterTargetSensor>(true);

            if (targetSensor != null && cameraLookController != null)
            {
                builder.RegisterComponent(targetSensor)
                    .As<ITargetCandidateSource>();

                builder.RegisterEntryPoint<CharacterTargetingService>(Lifetime.Scoped)
                    .As<ITargetProvider>();

                builder.RegisterEntryPoint<CharacterLabelPresenter>(Lifetime.Scoped);
            }

            if (cameraLookController != null)
            {
                builder.RegisterComponent(cameraLookController)
                    .AsSelf();
            }

            if (characterController != null)
            {
                builder.RegisterComponent(characterController)
                    .AsSelf();
            }
        }
    }
}
