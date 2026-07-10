using System;

namespace Game.Actor
{
    public interface IActorRuntime
    {
        Guid InstanceId { get; }

        ActorInstance Instance { get; }

        IActorView View { get; }

        IActorNavigation Navigation { get; }

        IActorDialogue Dialogue { get; }

        IActorInputBinder InputBinder { get; }

        IActorTargeting Targeting { get; }
    }

    public sealed class ActorRuntime :
        IActorRuntime
    {
        public ActorRuntime(
            ActorInstance instance,
            IActorView view,
            IActorNavigation navigation,
            IActorDialogue dialogue,
            IActorInputBinder inputBinder,
            IActorTargeting targeting)
        {
            Instance = instance
                ?? throw new ArgumentNullException(nameof(instance));

            View = view;
            Navigation = navigation;
            Dialogue = dialogue;
            InputBinder = inputBinder;
            Targeting = targeting;
        }

        public Guid InstanceId => Instance.InstanceId;

        public ActorInstance Instance { get; }

        public IActorView View { get; }

        public IActorNavigation Navigation { get; }

        public IActorDialogue Dialogue { get; }

        public IActorInputBinder InputBinder { get; }

        public IActorTargeting Targeting { get; }
    }
}