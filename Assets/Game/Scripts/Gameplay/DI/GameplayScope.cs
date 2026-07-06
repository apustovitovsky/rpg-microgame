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
        [SerializeField] private BuildConfiguratorSO _game;
        [SerializeField] private BuildConfiguratorSO _world;
        [SerializeField] private BuildConfiguratorSO _input;
        [SerializeField] private BuildConfiguratorSO _navigation;
        [SerializeField] private BuildConfiguratorSO _interaction;
        [SerializeField] private BuildConfiguratorSO _actor;
        [SerializeField] private BuildConfiguratorSO _player;
        [SerializeField] private BuildConfiguratorSO _actorNameplates;
        [SerializeField] private BuildConfiguratorSO _gameplay;
        [SerializeField] private BuildConfiguratorSO _pickup;

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