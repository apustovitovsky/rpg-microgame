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
        [SerializeField] private ModuleBuilder _game;
        [SerializeField] private ModuleBuilder _world;
        [SerializeField] private ModuleBuilder _input;
        [SerializeField] private ModuleBuilder _navigation;
        [SerializeField] private ModuleBuilder _interaction;
        [SerializeField] private ModuleBuilder _actor;
        [SerializeField] private ModuleBuilder _player;
        [SerializeField] private ModuleBuilder _actorNameplates;
        [SerializeField] private ModuleBuilder _gameplay;
        [SerializeField] private ModuleBuilder _pickup;

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