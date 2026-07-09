using Game.World;


namespace Game.Actor
{
    public interface IWorldActor
    {
        WorldId WorldId { get; }
        WorldInfo Info { get; }

        ActorDefinition Definition { get; }

        IActorTransform Transform { get; }

        IActorNavigation Navigation { get; }

        IActorDialogue Dialogue { get; }

        IActorInputBinder InputBinder { get; }

        IActorTargeting Targeting { get; }
    }

    public sealed class WorldActor :
        IWorldActor
    {
        public WorldActor(
            WorldInfo info,
            ActorDefinition definition,
            IActorTransform transform,
            IActorNavigation navigation,
            IActorDialogue dialogue,
            IActorInputBinder inputBinder,
            IActorTargeting targeting)
        {
            Info = info;
            Definition = definition;
            Transform = transform;
            InputBinder = inputBinder;
            Navigation = navigation;
            Dialogue = dialogue;
            Targeting = targeting;
        }

        public WorldInfo Info { get; }
        public WorldId WorldId => Info.WorldId;

        public string DisplayName =>
            !string.IsNullOrWhiteSpace(Info.DisplayName)
                ? Info.DisplayName
                : WorldId.ToString();

        public ActorDefinition Definition { get; }

        public IActorTransform Transform { get; }

        public IActorInputBinder InputBinder { get; }

        public IActorNavigation Navigation { get; }

        public IActorDialogue Dialogue { get; }
        public IActorTargeting Targeting { get; }
    }
}