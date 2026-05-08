namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletSimulationService
    {
        StoryletSimulationResult Simulate(StoryletSimulationRequest request);
    }
}
