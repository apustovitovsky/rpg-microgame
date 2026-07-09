using Game.Input;


namespace Game.Actor
{
    public sealed class ActorInputBinder :
        IActorInputBinder
    {
        private readonly ActorLookController _look;
        private readonly MovementController _movement;
        private readonly ActorTargetController _targeting;

        public ActorInputBinder(
            ActorLookController look,
            MovementController movement,
            ActorTargetController targeting)
        {
            _look = look;
            _movement = movement;
            _targeting = targeting;
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