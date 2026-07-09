using System;
using Game.Core;
using Game.Interaction;
using Game.Targeting;
using Game.World;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorWorldObjectFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly IWorldRegistry<IWorldActor> _actors;
        private readonly IWorldRegistry<IDisplayable> _displays;

        private readonly IWorldRegistry<ITargetProvider> _targetProviders;
        private readonly IWorldRegistry<IInteractable> _interactions;

        public ActorWorldObjectFactory(
            LifetimeScope parentScope,
            IWorldRegistry<IWorldActor> actors,
            IWorldRegistry<IDisplayable> displays,
            IWorldRegistry<ITargetProvider> targetProviders,
            IWorldRegistry<IInteractable> interactions)
        {
            _parentScope = parentScope;
            _actors = actors;
            _displays = displays;
            _targetProviders = targetProviders;
            _interactions = interactions;
        }

        public WorldSpawnResult Create(ActorSpawnRequest request)
        {
            if (request.WorldId.IsEmpty)
                throw new ArgumentException("Actor world id is required.", nameof(request));

            if (request.Definition == null)
                throw new ArgumentNullException(nameof(request.Definition));

            if (request.Definition.Prefab == null)
                throw new ArgumentNullException(nameof(request.Definition.Prefab));

            using (LifetimeScope.EnqueueParent(_parentScope))
            using (LifetimeScope.Enqueue(builder =>
            {
                builder.RegisterComponentInModuleRoot<ActorTarget>()
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithParameter(request.WorldId);
            }))
            {
                var instance = UnityEngine.Object.Instantiate(
                    request.Definition.Prefab,
                    request.Position,
                    request.Rotation,
                    request.Parent);

                instance.name = $"{request.Definition.DisplayName} ({request.WorldId})";

                var scope = instance.GetComponentInChildren<ActorScope>(true);

                if (scope == null)
                    throw new InvalidOperationException(
                        $"Actor prefab '{request.Definition.Prefab.name}' has no {nameof(ActorScope)}.");

                if (scope.Container == null)
                    throw new InvalidOperationException(
                        $"Actor prefab '{request.Definition.Prefab.name}' has no built VContainer scope.");

                var view = scope.Container.Resolve<IActorView>();

                scope.Container.TryResolve<IActorInputBinder>(out var inputBinder);
                scope.Container.TryResolve<IActorDialogue>(out var dialogue);
                scope.Container.TryResolve<IActorNavigation>(out var navigation);
                scope.Container.TryResolve<ITargetProvider>(out var targetProvider);
                scope.Container.TryResolve<IInteractable>(out var interaction);
                scope.Container.TryResolve<ActorTarget>(out var actorTarget);

                // actorTarget?.Initialize(request.WorldId);

                var actor = new WorldActor(
                    request.WorldId,
                    request.Definition,
                    view,
                    navigation,
                    dialogue,
                    inputBinder);

                var lifetime = new WorldLifetime(
                    request.WorldId,
                    view.Root.gameObject);

                lifetime.Add(_actors.Register(request.WorldId, actor));
                lifetime.Add(_displays.Register(request.WorldId, actor));

                if (targetProvider != null)
                    lifetime.Add(_targetProviders.Register(request.WorldId, targetProvider));

                if (interaction != null)
                    lifetime.Add(_interactions.Register(request.WorldId, interaction));

                return new WorldSpawnResult(lifetime);
            }
        }
    }
}