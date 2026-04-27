using UnityEngine;

namespace Etheria.Gameplay
{
    public interface IPickupPoolRoots
    {
        Transform ActiveRoot { get; }
        Transform InactiveRoot { get; }
    }
}