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
            var questStageInteractables =
                SceneComponentLookup.GetComponentsInScene<QuestStageInteractable>(builder);

            builder.RegisterComponentInHierarchy<DialogueRunner>();

            builder.RegisterInstance(_entryCatalog);

            builder.Register<DialogueService>(Lifetime.Singleton)
                .As<IDialogueService>();

            builder.RegisterComponentInHierarchy<DialogueView>();

            builder.Register<DialoguePresenter>(Lifetime.Singleton)
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterEntryPoint<DialogueInputHandler>(Lifetime.Singleton);

            builder.RegisterEntryPoint<QuestCommandHandler>(Lifetime.Singleton);
            builder.RegisterEntryPoint<WorldFactCommandHandler>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CharacterWorldCommandHandler>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<YarnDialoguePresenter>();

            builder.RegisterBuildCallback(container =>
            {
                foreach (var interactable in questStageInteractables)
                {
                    container.Inject(interactable);
                }
            });
        }
    }
}
