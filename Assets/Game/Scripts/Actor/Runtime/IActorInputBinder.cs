using Game.Input;

namespace Game.Actor
{
    public interface IActorInputBinder
    {
        void Bind(IActorInput input);
        void Unbind();
    }
}