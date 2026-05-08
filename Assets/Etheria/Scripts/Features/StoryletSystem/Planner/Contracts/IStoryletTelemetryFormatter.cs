namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletTelemetryFormatter
    {
        string Format(StoryletPlannerResult result);
    }
}
