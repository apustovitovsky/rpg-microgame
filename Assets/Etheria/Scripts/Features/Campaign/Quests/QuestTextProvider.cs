using Etheria.Game.Quests;
using UnityEngine.Localization.Settings;

namespace Etheria.Features.Campaign
{
    public sealed class QuestTextProvider : IQuestTextProvider
    {
        private const string TableName = "Quests";

        public string GetText(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            var result =
                LocalizationSettings.StringDatabase.GetTableEntry(
                    TableName,
                    key);

            var localizedText =
                result.Entry?.GetLocalizedString();

            return string.IsNullOrWhiteSpace(localizedText)
                ? key
                : localizedText;
        }
    }
}