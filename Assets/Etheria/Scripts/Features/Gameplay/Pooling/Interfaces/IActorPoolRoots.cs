using UnityEngine;

namespace Etheria.Gameplay
{
    public interface IActorPoolRoots
    {
        Transform ActiveRoot { get; }
        Transform InactiveRoot { get; }
    }
}