namespace Etheria.Game.Npc
{
    public interface INpcRuntime
    {
        string NpcId { get; }

        INpcTravelController Travel { get; }

        INpcDialogueStarter DialogueStarter { get; }

    }
}