using UnityEngine;

namespace Etheria.Game.Pooling
{
    public interface IActorPoolRoots
    {
        Transform ActiveRoot { get; }
        Transform InactiveRoot { get; }
    }
}
