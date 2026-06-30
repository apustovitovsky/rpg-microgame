using Etheria.Game.Npc;

namespace Etheria.Npc
{
    public sealed class NpcRuntime : INpcRuntime
    {
        private readonly NpcDefinitionSO _definition;
        private readonly INpcTravelController _travel;
        private readonly INpcDialogueStarter _dialogueStarter;

        public INpcDialogueStarter DialogueStarter =>
            _dialogueStarter;

        public NpcRuntime(
            NpcDefinitionSO definition,
            INpcTravelController travel,
            INpcDialogueStarter dialogueStarter)
        {
            _definition = definition;
            _travel = travel;
            _dialogueStarter = dialogueStarter;
        }

        public string NpcId =>
            _definition != null
                ? _definition.NpcId
                : string.Empty;

        public INpcTravelController Travel =>
            _travel;
    }
}