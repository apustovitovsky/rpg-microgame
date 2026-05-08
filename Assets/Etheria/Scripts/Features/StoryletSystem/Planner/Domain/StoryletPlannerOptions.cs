namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletPlannerOptions
    {
        public StoryletPlannerOptions(int? beamWidth = null, int? maxDepth = null)
        {
            BeamWidth = beamWidth;
            MaxDepth = maxDepth;
        }

        public int? BeamWidth { get; }
        public int? MaxDepth { get; }
    }
}
