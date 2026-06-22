using Etheria.Game.Character;
using UnityEngine;

namespace Etheria.Features.Character
{
    public sealed class CharacterIdentity :
        MonoBehaviour,
        ICharacterIdentity
    {
        [SerializeField] private CharacterDefinitionSO _definition;

        public string CharacterId =>
            _definition != null
                ? _definition.Id
                : string.Empty;

        public CharacterDefinitionSO Definition => _definition;
    }
}