using System;
using Game.Interaction;
using Game.Pickup;
using Game.Targeting;
using Game.World;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorWorldObjectFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly ActorWorldRegistrar _registrar;

        public ActorWorldObjectFactory(
            LifetimeScope parentScope,
            ActorWorldRegistrar registrar)
        {
            _parentScope = parentScope;
            _registrar = registrar;
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

                var actor = scope.Container.Resolve<WorldActor>();
                actor.Initialize(
                    request.WorldId,
                    request.Definition);

                var view = scope.Container.Resolve<IActorView>();

                scope.Container.TryResolve<IActorInputBinder>(out var inputBinder);
                scope.Container.TryResolve<ITargetProvider>(out var targetProvider);
                scope.Container.TryResolve<IInteractable>(out var interaction);
                scope.Container.TryResolve<IActorDialogue>(out var dialogue);
                scope.Container.TryResolve<IActorNavigation>(out var travel);
                scope.Container.TryResolve<IPickupEffectHandlerProvider>(out var pickupEffects);
                scope.Container.TryResolve<IInteractor>(out var interactor);

                var spawnedActor = new ActorSpawnedObject(
                    request.WorldId,
                    actor,
                    view,
                    actor,
                    interactor,
                    inputBinder,
                    targetProvider,
                    interaction,
                    dialogue,
                    travel,
                    pickupEffects);

                var lifetime = new WorldLifetime(
                    request.WorldId,
                    view.Root.gameObject);

                lifetime.Add(_registrar.Register(spawnedActor));

                return new WorldSpawnResult(lifetime);
            }
        }
    }
}