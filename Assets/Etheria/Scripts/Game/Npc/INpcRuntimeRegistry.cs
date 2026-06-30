namespace Etheria.Game.Npc
{
    public interface INpcRuntimeRegistry
    {
        bool TryGet(
            string npcId,
            out INpcRuntime runtime);
    }

    public interface INpcRuntimeRegistryWriter
    {
        void Register(INpcRuntime runtime);
        void Unregister(INpcRuntime runtime);
    }
}