using UnityEngine;
using VContainer;

namespace Etheria.Gameplay
{

    public sealed class ActorHitbox : MonoBehaviour
    {
        public ITargetable Targetable { get; private set; }

        public void Initialize(ITargetable targetable)
        {
            Targetable = targetable;
        }
    }
}

