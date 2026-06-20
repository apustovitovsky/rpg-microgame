using UnityEngine;

namespace Etheria.Game.Targeting
{
    public interface IPlayerTargetService : IPlayerTargetProvider
    {
        void SetTarget(Transform target);
        void Clear();
    }
}