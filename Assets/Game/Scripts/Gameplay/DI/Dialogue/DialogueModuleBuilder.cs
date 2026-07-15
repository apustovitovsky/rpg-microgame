using Game.Core;
using Game.Dialogue;
using Game.Dialogue.Yarn;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Yarn.Unity;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "DialogueModuleBuilder",
        menuName = "Game/Gameplay/Dialogue Module Builder")]
    public sealed class DialogueModuleBuilder :
        ModuleBuilder
    {
        public override void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<DialogueRunner>();

            builder.RegisterComponentInHierarchy<DialogueView>()
                .AsImplementedInterfaces();

            builder.RegisterComponentInHierarchy<
                YarnDialoguePresenter>();

            builder.Register<DialoguePresenter>(
                Lifetime.Singleton);

            builder.Register<YarnDialogueExecutor>(
                    Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<DialogueCoordinator>(
                    Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}