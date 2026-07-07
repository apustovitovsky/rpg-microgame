using System;
using Game.Interaction;
using Game.Pickup;
using Game.Targeting;
using Game.World;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Actor
{
    public sealed class ActorWorldObjectFactory :
        IWorldObjectFactory<ActorSpawnRequest>
    {
        private readonly LifetimeScope _parentScope;

        public ActorWorldObjectFactory(LifetimeScope parentScope)
        {
            _parentScope = parentScope;
        }

        public IWorldObject Create(ActorSpawnRequest request)
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
                {
                    throw new InvalidOperationException(
                        $"Actor prefab '{request.Prefab.name}' has no {nameof(ActorScope)}.");
                }

                if (scope.Container == null)
                {
                    throw new InvalidOperationException(
                        $"Actor prefab '{request.Prefab.name}' has no built VContainer scope.");
                }

                var identity = scope.Container.Resolve<IActorIdentity>()
                    ?? throw new InvalidOperationException(
                        $"Actor prefab '{request.Prefab.name}' has no {nameof(IActorIdentity)}.");

                identity.Initialize(
                    request.WorldId,
                    displayName);

                var view = scope.Container.Resolve<IActorView>();

                var builder = new WorldObjectBuilder()
                    .Add<IActorView>(view);

                if (scope.Container.TryResolve<IActorInputBinder>(out var inputBinder))
                    builder.Add<IActorInputBinder>(inputBinder);

                if (scope.Container.TryResolve<ITargetProvider>(out var targetProvider))
                    builder.Add<ITargetProvider>(targetProvider);

                if (scope.Container.TryResolve<IInteractable>(out var interaction))
                    builder.Add<IInteractable>(interaction);

                if (scope.Container.TryResolve<IActorDialogueEndpoint>(out var dialogue))
                    builder.Add<IActorDialogueEndpoint>(dialogue);

                if (scope.Container.TryResolve<IActorTravelEndpoint>(out var travel))
                    builder.Add<IActorTravelEndpoint>(travel);

                if (scope.Container.TryResolve<IPickupEffectHandlerProvider>(out var pickupEffects))
                    builder.Add<IPickupEffectHandlerProvider>(pickupEffects);

                return builder.Build(
                    request.WorldId,
                    view.Root.gameObject);
            }
        }
    }
}