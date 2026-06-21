using UnityEngine;

namespace Etheria.Features.Targeting
{
    [DisallowMultipleComponent]
    public sealed class TargetableHitbox : MonoBehaviour, ITargetableProvider
    {
        [SerializeField] private Targetable _targetable;

        public ITargetable Targetable => _targetable;

        private void Reset()
        {
            _targetable = GetComponentInParent<Targetable>();
        }
    }
}
