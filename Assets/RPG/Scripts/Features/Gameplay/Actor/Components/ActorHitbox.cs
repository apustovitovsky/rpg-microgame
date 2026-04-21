using UnityEngine;

namespace RPG.Gameplay
{

    public sealed class ActorHitbox : MonoBehaviour
    {
        public IActorTargetable Targetable { get; private set; }

        public void Initialize(IActorTargetable targetable)
        {
            Targetable = targetable;
        }
    }
}

