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
        [SerializeField] private BuildConfiguratorSO _identity;
        [SerializeField] private BuildConfiguratorSO _movement;
        [SerializeField] private BuildConfiguratorSO _targeting;
        [SerializeField] private BuildConfiguratorSO _combat;
        [SerializeField] private BuildConfiguratorSO _ai;

        protected override void Configure(IContainerBuilder builder)
        {
            var root = _actorRoot != null
                ? _actorRoot
                : transform;

            builder.RegisterInstance(new ScopeRoot(root));

            builder.Register<ActorInstanceFactory>(Lifetime.Scoped);

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