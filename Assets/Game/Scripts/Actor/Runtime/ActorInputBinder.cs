using System;
using System.Collections.Generic;
using Game.Input;
using Game.World;

namespace Game.Actor
{
    public sealed class ActorInputBinder :
        IActorInputBinder,
        IWorldCapability
    {
        private readonly ActorLookController _look;
        private readonly MovementController _movement;
        private readonly TargetingController _targeting;

        public ActorInputBinder(
            ActorLookController look,
            MovementController movement,
            TargetingController targeting)
        {
            _look = look;
            _movement = movement;
            _targeting = targeting;
        }

        public IEnumerable<Type> PublishedTypes
        {
            get { yield return typeof(IActorInputBinder); }
        }

        public void Bind(IActorInput input)
        {
            _look.Bind(input);
            _movement.Bind(input);
            _targeting.Bind(input);
        }

        public void Unbind()
        {
            _look.Unbind();
            _movement.Unbind();
            _targeting.Unbind();
        }
    }
}