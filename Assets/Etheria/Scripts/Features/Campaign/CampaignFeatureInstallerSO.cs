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
    public sealed class CampaignFeatureInstallerSO : InstallerSO
    {
        public override void Install(IContainerBuilder builder)
        {
            var dialogueInteractables =
                SceneComponentLookup.FindAll<NpcDialogueInteractable>(builder);

            var questCompletionInteractables =
                SceneComponentLookup.FindAll<QuestCompletionInteractable>(builder);

            builder.RegisterComponentInHierarchy<DialogueRunner>();

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
