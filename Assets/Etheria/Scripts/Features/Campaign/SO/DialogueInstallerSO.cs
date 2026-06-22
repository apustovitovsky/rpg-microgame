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
        fileName = "DialogueInstaller",
        menuName = "Etheria/Features/Campaign/Dialogue Installer")]
    public sealed class DialogueInstallerSO : InstallerSO
    {
        [SerializeField] private DialogueEntryCatalogSO _entryCatalog;

        public override void Install(IContainerBuilder builder)
        {
            var dialogueInteractables =
                SceneComponentLookup.FindAll<NpcDialogueInteractable>(builder);

            var questStageInteractables =
                SceneComponentLookup.FindAll<QuestStageInteractable>(builder);

            builder.RegisterComponentInHierarchy<DialogueRunner>();

            builder.RegisterInstance(_entryCatalog);
            
            builder.Register<DialogueService>(Lifetime.Singleton)
                .As<IDialogueService>();

            builder.RegisterEntryPoint<QuestCommandHandler>(
                Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<DialogueSpeakerPresenter>();

            builder.RegisterBuildCallback(container =>
            {
                foreach (var interactable in dialogueInteractables)
                {
                    container.Inject(interactable);
                }
                foreach (var interactable in questStageInteractables)
                {
                    container.Inject(interactable);
                }
            });
        }
    }
}
