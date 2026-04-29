namespace Etheria.Game.Actor
{
    public interface IActorIdentity
    {
        ActorId Id { get; }
        string DisplayName { get; }
    }
}