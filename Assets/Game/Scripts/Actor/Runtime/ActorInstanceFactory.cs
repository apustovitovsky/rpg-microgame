using Game.Targeting;
using VContainer;

namespace Game.Actor
{
    public sealed class ActorInstanceFactory
    {
        private readonly IObjectResolver _resolver;

        public ActorInstanceFactory(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public ActorInstance Create(
            string instanceId,
            string definitionId)
        {
            var view = _resolver.Resolve<IActorView>();

            _resolver.TryResolve<IActorTravelEndpoint>(
                out var travel);

            _resolver.TryResolve<IActorDialogueHandler>(
                out var dialogue);

            _resolver.TryResolve<IActorCombatHandler>(
                out var combat);

            _resolver.TryResolve<ITargetProvider>(
                out var targetProvider);

            _resolver.TryResolve<IActorInputBinder>(
                out var inputBinder);

            return new(
                instanceId,
                definitionId,
                view,
                travel,
                targetProvider,
                inputBinder,
                dialogue,
                combat);
        }
    }
}