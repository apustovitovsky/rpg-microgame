namespace Etheria.Features.NarrativeGeneration
{
    public static class EventEvaluator
    {
        public static int Evaluate(
            in WorldState worldState,
            in CompiledEventDefinition eventDefinition,
            in RelationContext participantContext)
        {
            // Minimal prototype evaluation:
            // any successfully matched participant context is considered valid.
            return participantContext.Slot1.IsValid ? 1 : 0;
        }
    }
}
