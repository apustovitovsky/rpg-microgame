using Game.Targeting;

namespace Game.Actor
{
    public sealed class ActorInstance
    {
        public ActorInstance(
            string instanceId,
            string definitionId,
            IActorView view,
            IActorTravelEndpoint travel = null,
            ITargetProvider targetProvider = null,
            IActorInputBinder inputBinder = null,
            IActorDialogueHandler dialogue = null,
            IActorCombatHandler combat = null)
        {
            InstanceId = instanceId;
            DefinitionId = definitionId;
            View = view;
            Travel = travel;
            TargetProvider = targetProvider;
            InputBinder = inputBinder;
            Dialogue = dialogue;
            Combat = combat;
        }

        public string InstanceId { get; }
        public string DefinitionId { get; }

        public IActorView View { get; }
        public IActorTravelEndpoint Travel { get; }
        public ITargetProvider TargetProvider { get; }
        public IActorInputBinder InputBinder { get; }
        public IActorDialogueHandler Dialogue { get; }
        public IActorCombatHandler Combat { get; }
    }
}