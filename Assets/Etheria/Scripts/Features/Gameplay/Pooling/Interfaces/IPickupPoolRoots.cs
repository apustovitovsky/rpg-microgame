using UnityEngine;

namespace Etheria.Features.Gameplay
{
    public interface IPickupPoolRoots
    {
        Transform ActiveRoot { get; }
        Transform InactiveRoot { get; }
    }
}
