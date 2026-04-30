using Etheria.Game.Targeting;
using UnityEngine;

namespace Etheria.Features.Actor
{
    public sealed class ActorHitbox : MonoBehaviour, ITargetableProvider
    {
        public ITargetable Targetable { get; private set; }

        public void Initialize(ITargetable targetable)
        {
            Targetable = targetable;
        }
    }
}
