namespace Etheria.Game.Npc
{
    public interface INpcAgentRegistry
    {
        bool TryGet(
            string npcId,
            out INpcAgent agent);
    }

    public interface INpcAgentRegistryWriter
    {
        void Register(INpcAgent agent);
        void Unregister(INpcAgent agent);
    }
}