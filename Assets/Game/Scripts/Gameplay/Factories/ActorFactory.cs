using System;
using Game.Core;
using Game.Interaction;
using Game.Targeting;
using Game.World;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorFactory
    {
        private readonly LifetimeScope _parentScope;
        private readonly IActorRegistrationService _actors;
        private readonly IInteractionRegistrationService _interactions;

        public ActorFactory(
            LifetimeScope parentScope,
            IActorRegistrationService actors,
            IInteractionRegistrationService interactions)
        {
            _parentScope = parentScope;
            _actors = actors;
            _interactions = interactions;
        }

        public IWorldObject Create(ActorSpawnRequest request)
        {
            if (request.WorldId.IsEmpty)
                throw new ArgumentException("Actor world id is required.", nameof(request));

            if (request.Definition == null)
                throw new ArgumentNullException(nameof(request.Definition));

            if (request.Definition.Prefab == null)
                throw new ArgumentNullException(nameof(request.Definition.Prefab));

            var info = new WorldInfo(
                request.WorldId,
                request.Definition.DisplayName);

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

                if (scope.Container.TryResolve<Targetable>(out var targetable))
                    targetable.Initialize(info);

                var view = scope.Container.Resolve<IActorTransform>();

                scope.Container.TryResolve<IActorInputBinder>(out var inputBinder);
                scope.Container.TryResolve<IActorDialogue>(out var dialogue);
                scope.Container.TryResolve<IActorNavigation>(out var navigation);
                scope.Container.TryResolve<IActorTargeting>(out var targeting);
                scope.Container.TryResolve<IInteractable>(out var interaction);

                var actor = new WorldActor(
                    info,
                    request.Definition,
                    view,
                    navigation,
                    dialogue,
                    inputBinder,
                    targeting);

                var lifetime = new WorldObject(
                    view.Root.gameObject,
                    info);

                lifetime.Add(_actors.Register(actor));

                if (interaction != null)
                    lifetime.Add(_interactions.RegisterInteractable(request.WorldId, interaction));

                return lifetime;
            }
        }
    }
}