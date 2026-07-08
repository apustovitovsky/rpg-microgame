using System;
using Game.Interaction;
using Game.Pickup;
using Game.Targeting;
using Game.World;

namespace Game.Actor
{
    public sealed class ActorWorldRegistrar
    {
        private readonly IWorldRegistry<IWorldActor> _actors;
        private readonly IWorldRegistry<IActorView> _view;
        private readonly IWorldRegistry<IDisplayable> _displayInfos;
        private readonly IWorldRegistry<IInteractor> _interactors;
        private readonly IWorldRegistry<IActorInputBinder> _inputBinders;
        private readonly IWorldRegistry<ITargetProvider> _targetProviders;
        private readonly IWorldRegistry<IInteractable> _interactions;
        private readonly IWorldRegistry<IActorDialogue> _dialogues;
        private readonly IWorldRegistry<IActorNavigation> _travels;
        private readonly IWorldRegistry<IPickupEffectHandlerProvider> _pickupEffectHandlers;

        public ActorWorldRegistrar(
            IWorldRegistry<IWorldActor> actors,
            IWorldRegistry<IActorView> view,
            IWorldRegistry<IDisplayable> displayInfos,
            IWorldRegistry<IInteractor> interactors,
            IWorldRegistry<IActorInputBinder> inputBinders,
            IWorldRegistry<ITargetProvider> targetProviders,
            IWorldRegistry<IInteractable> interactions,
            IWorldRegistry<IActorDialogue> dialogues,
            IWorldRegistry<IActorNavigation> travels,
            IWorldRegistry<IPickupEffectHandlerProvider> pickupEffectHandlers)
        {
            _actors = actors;
            _view = view;
            _displayInfos = displayInfos;
            _interactors = interactors;
            _inputBinders = inputBinders;
            _targetProviders = targetProviders;
            _interactions = interactions;
            _dialogues = dialogues;
            _travels = travels;
            _pickupEffectHandlers = pickupEffectHandlers;
        }

        public IRegistrationToken Register(ActorSpawnedObject actor)
        {
            if (actor == null)
                throw new ArgumentNullException(nameof(actor));

            var lifetime = new CompositeRegistration();

            lifetime.Add(_actors.Register(actor.WorldId, actor.Actor));
            lifetime.Add(_view.Register(actor.WorldId, actor.View));
            lifetime.Add(_displayInfos.Register(actor.WorldId, actor.DisplayInfo));

            if (actor.Interactor != null)
                lifetime.Add(_interactors.Register(actor.WorldId, actor.Interactor));

            if (actor.InputBinder != null)
                lifetime.Add(_inputBinders.Register(actor.WorldId, actor.InputBinder));

            if (actor.TargetProvider != null)
                lifetime.Add(_targetProviders.Register(actor.WorldId, actor.TargetProvider));

            if (actor.Interaction != null)
                lifetime.Add(_interactions.Register(actor.WorldId, actor.Interaction));

            if (actor.Dialogue != null)
                lifetime.Add(_dialogues.Register(actor.WorldId, actor.Dialogue));

            if (actor.Travel != null)
                lifetime.Add(_travels.Register(actor.WorldId, actor.Travel));

            if (actor.PickupEffectHandlerProvider != null)
                lifetime.Add(_pickupEffectHandlers.Register(actor.WorldId, actor.PickupEffectHandlerProvider));

            return lifetime;
        }
    }
}