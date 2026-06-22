using Etheria.Game.Character;
using UnityEngine.Localization.Settings;

namespace Etheria.Features.Character
{
    public sealed class CharacterNameProvider : ICharacterNameProvider
    {
        private const string TableName = "Characters";

        public string GetDisplayName(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
                return string.Empty;

            var result = LocalizationSettings.StringDatabase.GetTableEntry(
                TableName,
                characterId);

            var localizedName = result.Entry?.GetLocalizedString();

            return string.IsNullOrWhiteSpace(localizedName)
                ? characterId
                : localizedName;
        }
    }
}