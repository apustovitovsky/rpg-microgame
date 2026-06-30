namespace Etheria.Game.Npc
{
    public interface INpcStateRegistry
    {
        NpcState GetOrCreate(
            string npcId);

        bool TryGet(
            string npcId,
            out NpcState state);
    }
}