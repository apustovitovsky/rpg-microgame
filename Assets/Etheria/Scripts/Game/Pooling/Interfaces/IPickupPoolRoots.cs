using UnityEngine;

namespace Etheria.Game.Pooling
{
    public interface IPickupPoolRoots
    {
        Transform ActiveRoot { get; }
        Transform InactiveRoot { get; }
    }
}
