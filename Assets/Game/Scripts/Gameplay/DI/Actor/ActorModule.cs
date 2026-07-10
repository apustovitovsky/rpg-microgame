using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorModule : LifetimeScope
    {
        [SerializeField] private Transform _actorRoot;

        [Header("Build Configurations")]
        [SerializeField] private ModuleBuilder _identity;
        [SerializeField] private ModuleBuilder _commands;
        [SerializeField] private ModuleBuilder _inventory;
        [SerializeField] private ModuleBuilder _movement;
        [SerializeField] private ModuleBuilder _targeting;
        [SerializeField] private ModuleBuilder _combat;
        [SerializeField] private ModuleBuilder _ai;

        protected override void Configure(IContainerBuilder builder)
        {
            var root = _actorRoot != null
                ? _actorRoot
                : transform;

            builder.RegisterInstance(new ModuleRoot(root));

            builder.Register<ActorInputBinder>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Configure(_identity);
            builder.Configure(_commands);
            builder.Configure(_inventory);
            builder.Configure(_movement);
            builder.Configure(_targeting);
            builder.Configure(_combat);
            builder.Configure(_ai);
        }
    }
}