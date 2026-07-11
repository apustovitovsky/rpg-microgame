using Game.Input;

namespace Game.Control
{
    public interface IControlInputBinder
    {
        void Bind(IControlInput input);

        void Unbind();
    }
}