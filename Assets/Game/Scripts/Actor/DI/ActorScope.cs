using Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    [DisallowMultipleComponent]
    public sealed class ActorScope : LifetimeScope
    {
        [SerializeField] private Transform _actorRoot;

        [Header("Build Configurations")]
        [SerializeField] private BuildConfigurator _identity;
        [SerializeField] private BuildConfigurator _movement;
        [SerializeField] private BuildConfigurator _targeting;
        [SerializeField] private BuildConfigurator _combat;
        [SerializeField] private BuildConfigurator _ai;

        protected override void Configure(IContainerBuilder builder)
        {
            var root = _actorRoot != null
                ? _actorRoot
                : transform;

            builder.RegisterInstance(new ScopeRoot(root));

            builder.Register<ActorInputBinder>(Lifetime.Scoped)
                .AsImplementedInterfaces();

            builder.Configure(_identity);
            builder.Configure(_movement);
            builder.Configure(_targeting);
            builder.Configure(_combat);
            builder.Configure(_ai);
        }
    }
}