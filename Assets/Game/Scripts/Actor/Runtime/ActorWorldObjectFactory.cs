using System;
using Game.Interaction;
using Game.Pickup;
using Game.Targeting;
using Game.World;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorWorldObjectFactory :
        IWorldObjectFactory<ActorSpawnRequest>
    {
        private readonly LifetimeScope _parentScope;
        private readonly IWorldRegistry<IWorldObject> _worldObjects;
        private readonly IWorldRegistry<IWorldActor> _actors;
        private readonly IWorldRegistry<IActorInputBinder> _inputBinders;
        private readonly IWorldRegistry<ITargetProvider> _targetProviders;
        private readonly IWorldRegistry<IInteractable> _interactions;
        private readonly IWorldRegistry<IActorDialogueEndpoint> _dialogues;
        private readonly IWorldRegistry<IActorTravelEndpoint> _travels;
        private readonly IWorldRegistry<IPickupEffectHandlerProvider> _pickupEffectHandlers;

        public ActorWorldObjectFactory(
            LifetimeScope parentScope,
            IWorldRegistry<IWorldObject> worldObjects,
            IWorldRegistry<IWorldActor> actors,
            IWorldRegistry<IActorInputBinder> inputBinders,
            IWorldRegistry<ITargetProvider> targetProviders,
            IWorldRegistry<IInteractable> interactions,
            IWorldRegistry<IActorDialogueEndpoint> dialogues,
            IWorldRegistry<IActorTravelEndpoint> travels,
            IWorldRegistry<IPickupEffectHandlerProvider> pickupEffectHandlers)
        {
            _parentScope = parentScope;
            _worldObjects = worldObjects;
            _actors = actors;
            _inputBinders = inputBinders;
            _targetProviders = targetProviders;
            _interactions = interactions;
            _dialogues = dialogues;
            _travels = travels;
            _pickupEffectHandlers = pickupEffectHandlers;
        }

        public WorldSpawnResult Create(ActorSpawnRequest request)
        {
            var displayName = request.DisplayName?.Trim() ?? string.Empty;

            if (request.WorldId.IsEmpty)
                throw new ArgumentException("Actor world id is required.", nameof(request));

            if (request.Prefab == null)
                throw new ArgumentNullException(nameof(request.Prefab));

            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var instance = UnityEngine.Object.Instantiate(
                    request.Prefab,
                    request.Position,
                    request.Rotation,
                    request.Parent);

                instance.name = string.IsNullOrWhiteSpace(displayName)
                    ? request.WorldId.ToString()
                    : $"{displayName} ({request.WorldId})";

                var scope = instance.GetComponentInChildren<ActorScope>(true);

                if (scope == null)
                    throw new InvalidOperationException(
                        $"Actor prefab '{request.Prefab.name}' has no {nameof(ActorScope)}.");

                if (scope.Container == null)
                    throw new InvalidOperationException(
                        $"Actor prefab '{request.Prefab.name}' has no built VContainer scope.");

                var identity = scope.Container.Resolve<IActorIdentity>()
                    ?? throw new InvalidOperationException(
                        $"Actor prefab '{request.Prefab.name}' has no {nameof(IActorIdentity)}.");

                identity.Initialize(
                    request.WorldId,
                    displayName);

                var view = scope.Container.Resolve<IWorldActor>();

                var worldObject = new WorldObject(
                    request.WorldId,
                    view.Root.gameObject);

                var lifetime = new CompositeRegistration();

                lifetime.Add(_worldObjects.Register(request.WorldId, worldObject));
                lifetime.Add(_actors.Register(request.WorldId, view));

                if (scope.Container.TryResolve<IActorInputBinder>(out var inputBinder))
                    lifetime.Add(_inputBinders.Register(request.WorldId, inputBinder));

                if (scope.Container.TryResolve<ITargetProvider>(out var targetProvider))
                    lifetime.Add(_targetProviders.Register(request.WorldId, targetProvider));

                if (scope.Container.TryResolve<IInteractable>(out var interaction))
                    lifetime.Add(_interactions.Register(request.WorldId, interaction));

                if (scope.Container.TryResolve<IActorDialogueEndpoint>(out var dialogue))
                    lifetime.Add(_dialogues.Register(request.WorldId, dialogue));

                if (scope.Container.TryResolve<IActorTravelEndpoint>(out var travel))
                    lifetime.Add(_travels.Register(request.WorldId, travel));

                if (scope.Container.TryResolve<IPickupEffectHandlerProvider>(out var pickupEffects))
                    lifetime.Add(_pickupEffectHandlers.Register(request.WorldId, pickupEffects));

                return new WorldSpawnResult(
                    worldObject,
                    lifetime);
            }
        }
    }
}