using UnityEngine;

namespace Etheria.Features.Character
{
    [CreateAssetMenu(
        fileName = "CharacterDefinition",
        menuName = "Etheria/Character/Definition")]
    public sealed class CharacterDefinitionSO : ScriptableObject
    {
        [field: SerializeField]
        public string Id { get; private set; }

        private void OnValidate()
        {
            Id = Id?.Trim();
        }
    }
}