using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Core
{
    [DisallowMultipleComponent]
    public sealed class WorldScope : LifetimeScope
    {
        [Header("Build Configurations")]
        [SerializeField] private BuildConfiguratorSO _game;
        [SerializeField] private BuildConfiguratorSO _input;
        [SerializeField] private BuildConfiguratorSO _navigation;
        [SerializeField] private BuildConfiguratorSO _actor;
        [SerializeField] private BuildConfiguratorSO _player;
        [SerializeField] private BuildConfiguratorSO _actorNameplates;
        [SerializeField] private BuildConfiguratorSO _world;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Configure(_game);
            builder.Configure(_input);
            builder.Configure(_navigation);
            builder.Configure(_actor);
            builder.Configure(_player);
            builder.Configure(_actorNameplates);
            builder.Configure(_world);
        }
    }
}