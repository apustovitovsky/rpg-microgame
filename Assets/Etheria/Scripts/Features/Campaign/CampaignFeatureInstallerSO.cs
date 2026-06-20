using System;
using Etheria.Core.DI;
using Etheria.Game.Dialogue;
using Etheria.Game.Quests;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Yarn.Unity;

namespace Etheria.Features.Campaign
{
    [CreateAssetMenu(
        fileName = "CampaignFeatureInstaller",
        menuName = "Etheria/Features/Campaign/Campaign Feature Installer")]
    public sealed class CampaignFeatureInstallerSO : ScopeInstallerSO
    {
        public override void Install(
            IContainerBuilder builder,
            GameObject rootObject)
        {


            var dialogueInteractables =
                rootObject.GetComponentsInChildren<NpcDialogueInteractable>(true);

            var questCompletionInteractables =
                rootObject.GetComponentsInChildren<QuestCompletionInteractable>(true);

            builder.RegisterComponentInHierarchy<DialogueRunner>()
                .UnderTransform(rootObject.transform);

            builder.Register<DialogueService>(Lifetime.Singleton)
                .As<IDialogueService>();

            builder.Register<QuestService>(Lifetime.Singleton)
                .As<IQuestService>();

            builder.RegisterEntryPoint<QuestCommandHandler>(
                Lifetime.Singleton);

            builder.RegisterBuildCallback(container =>
            {
                foreach (var interactable in dialogueInteractables)
                {
                    container.Inject(interactable);
                }
                foreach (var interactable in questCompletionInteractables)
                {
                    container.Inject(interactable);
                }
            });
        }
    }
}