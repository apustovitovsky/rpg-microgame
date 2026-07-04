using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Core
{
    [DisallowMultipleComponent]
    public sealed class WorldScope : LifetimeScope
    {
        [Header("Build Configurations")]
        [SerializeField] private BuildConfigurationSO _game;
        [SerializeField] private BuildConfigurationSO _input;
        [SerializeField] private BuildConfigurationSO _navigation;
        [SerializeField] private BuildConfigurationSO _actor;
        [SerializeField] private BuildConfigurationSO _player;
        [SerializeField] private BuildConfigurationSO _possession;
        [SerializeField] private BuildConfigurationSO _actorNameplates;
        [SerializeField] private BuildConfigurationSO _world;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Configure(_game);
            builder.Configure(_input);
            builder.Configure(_navigation);
            builder.Configure(_actor);
            builder.Configure(_player);
            builder.Configure(_possession);
            builder.Configure(_actorNameplates);
            builder.Configure(_world);
        }
    }
}