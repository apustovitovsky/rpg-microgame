namespace Game.Core
{
    public interface IEventPublisher
    {
        void Publish<TEvent>(TEvent eventData);
    }
}