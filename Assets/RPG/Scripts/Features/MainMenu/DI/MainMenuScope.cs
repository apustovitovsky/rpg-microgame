using RPG.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RPG.MainMenu
{
    public class MainMenuScope : LifetimeScope
    {
        [SerializeField] private MainMenuConfigSO _mainMenuConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_mainMenuConfig);

            builder.RegisterEntryPoint<MainMenuSceneInitiator>(Lifetime.Scoped)
                .As<ISceneInitiator>();

            builder.RegisterComponentInNewPrefab(_mainMenuConfig.MainMenuView, Lifetime.Scoped);
            builder.RegisterEntryPoint<MainMenuPresenter>(Lifetime.Scoped);
        }
    }
}
