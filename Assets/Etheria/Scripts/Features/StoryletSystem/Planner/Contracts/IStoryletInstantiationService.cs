namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletInstantiationService
    {
        StoryletInstantiationResult TryInstantiate(
            StoryletDefinition definition,
            StoryletWorldState worldState,
            StoryletPlannerMemory memory);
    }
}
