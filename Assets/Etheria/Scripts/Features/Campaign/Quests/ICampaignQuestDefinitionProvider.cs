namespace Etheria.Features.Campaign
{
    public interface ICampaignQuestDefinitionProvider
    {
        bool TryGetTravelInstruction(
            string questId,
            string instructionId,
            out QuestTravelInstruction instruction);
    }
}