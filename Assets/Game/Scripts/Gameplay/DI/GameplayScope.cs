using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class GameplayScope : LifetimeScope
    {
        [Header("Build Configurations")]
        [SerializeField] private BuildConfigurator _game;
        [SerializeField] private BuildConfigurator _world;
        [SerializeField] private BuildConfigurator _input;
        [SerializeField] private BuildConfigurator _navigation;
        [SerializeField] private BuildConfigurator _interaction;
        [SerializeField] private BuildConfigurator _actor;
        [SerializeField] private BuildConfigurator _player;
        [SerializeField] private BuildConfigurator _actorNameplates;
        [SerializeField] private BuildConfigurator _gameplay;
        [SerializeField] private BuildConfigurator _pickup;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Configure(_game);
            builder.Configure(_world);
            builder.Configure(_input);
            builder.Configure(_navigation);
            builder.Configure(_interaction);
            builder.Configure(_actor);
            builder.Configure(_player);
            builder.Configure(_actorNameplates);
            builder.Configure(_gameplay);
            builder.Configure(_pickup);
        }
    }
}