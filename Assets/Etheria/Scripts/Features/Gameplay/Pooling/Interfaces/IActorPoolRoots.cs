using UnityEngine;

namespace Etheria.Features.Gameplay
{
    public interface IActorPoolRoots
    {
        Transform ActiveRoot { get; }
        Transform InactiveRoot { get; }
    }
}
