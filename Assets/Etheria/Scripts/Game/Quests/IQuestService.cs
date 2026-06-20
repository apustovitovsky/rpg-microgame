namespace Etheria.Game.Quests
{
    public interface IQuestService
    {
        bool TryStart(string questId);
        bool TryComplete(string questId);
        QuestStatus GetStatus(string questId);
    }
}