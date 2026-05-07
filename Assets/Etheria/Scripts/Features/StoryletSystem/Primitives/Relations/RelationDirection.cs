namespace Etheria.Features.StoryletSystem
{
    public enum RelationDirection : byte
    {
        FromSelfToTarget = 0,
        FromTargetToSelf = 1,
        BothDirections = 2,
        AnyDirection = 3,
    }
}
