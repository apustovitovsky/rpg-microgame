using Game.World;


namespace Game.Actor
{
    public sealed class WorldActor :
        IWorldActor,
        IDisplayable
    {
        public WorldActor(
            WorldId worldId,
            ActorDefinition definition,
            IActorView view,
            IActorNavigation navigation,
            IActorDialogue dialogue,
            IActorInputBinder inputBinder)
        {
            WorldId = worldId;
            Definition = definition;
            View = view;
            InputBinder = inputBinder;
            Navigation = navigation;
            Dialogue = dialogue;
        }

        public WorldId WorldId { get; }

        public ActorDefinition Definition { get; }

        public IActorView View { get; }

        public IActorInputBinder InputBinder { get; }

        public IActorNavigation Navigation { get; }

        public IActorDialogue Dialogue { get; }

        public string DisplayName =>
            Definition != null && !string.IsNullOrWhiteSpace(Definition.DisplayName)
                ? Definition.DisplayName
                : WorldId.ToString();
    }
}