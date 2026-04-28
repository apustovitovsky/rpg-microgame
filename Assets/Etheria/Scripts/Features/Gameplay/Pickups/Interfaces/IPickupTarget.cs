

namespace Etheria.Features.Gameplay
{
    public interface IPickupTarget
    {
        bool TryGet<T>(out T service) where T : class;
    }
}

