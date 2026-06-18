namespace Etheria.Game.Actor
{
    public interface IActorCapabilityProvider
    {
        bool TryGet<T>(out T capability) where T : class;
    }
}