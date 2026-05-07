namespace Etheria.Features.StoryletSystem
{
    public interface IStoryletEffectApplier
    {
        StoryletWorldState Apply(
            StoryletInstantiationCandidate candidate,
            StoryletWorldState worldState);
    }
}
