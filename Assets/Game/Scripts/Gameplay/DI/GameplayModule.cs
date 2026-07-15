using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class GameplayModule :
        LifetimeScope
    {
        [SerializeField] private ModuleBuilder _game;
        [SerializeField] private ModuleBuilder _world;
        [SerializeField] private ModuleBuilder _commands;
        [SerializeField] private ModuleBuilder _dialogue;
        [SerializeField] private ModuleBuilder _inventory;
        [SerializeField] private ModuleBuilder _loot;
        [SerializeField] private ModuleBuilder _input;
        [SerializeField] private ModuleBuilder _navigation;
        [SerializeField] private ModuleBuilder _actor;
        [SerializeField] private ModuleBuilder _nameplates;
        [SerializeField] private ModuleBuilder _player;
        [SerializeField] private ModuleBuilder _pickup;
        [SerializeField] private ModuleBuilder _gameplay;

        protected override void Configure(
            IContainerBuilder builder)
        {
            builder.Configure(_game);
            builder.Configure(_world);
            builder.Configure(_commands);
            builder.Configure(_dialogue);
            builder.Configure(_inventory);
            builder.Configure(_loot);
            builder.Configure(_input);
            builder.Configure(_navigation);
            builder.Configure(_actor);
            builder.Configure(_nameplates);
            builder.Configure(_player);
            builder.Configure(_pickup);
            builder.Configure(_gameplay);
        }
    }
}