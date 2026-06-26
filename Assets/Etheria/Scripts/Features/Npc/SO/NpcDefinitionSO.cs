using UnityEngine;

namespace Etheria.Npc
{
    [CreateAssetMenu(
        fileName = "NpcDefinition",
        menuName = "Etheria/Npc/Npc Definition")]
    public sealed class NpcDefinitionSO : ScriptableObject
    {
        [field: SerializeField]
        public string NpcId { get; private set; }

        [field: SerializeField]
        public GameObject Prefab { get; private set; }

        private void OnValidate()
        {
            NpcId = NpcId?.Trim();
        }
    }
}