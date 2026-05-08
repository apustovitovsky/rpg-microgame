namespace Etheria.Features.StoryletSystem.Authoring
{
    public enum StoryletRepeatabilityMode : byte
    {
        OnceEver = 0,
        OncePerRun = 1,
        RepeatableWithCooldown = 2,
        RepeatableUntilStateChanges = 3,
    }
}
