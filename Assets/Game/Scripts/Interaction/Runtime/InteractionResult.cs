namespace Game.Interaction
{
    public enum InteractionResult
    {
        Succeeded = 0,

        InvalidInteractor = 1,
        InvalidTarget = 2,
        SameObject = 3,

        InteractableNotFound = 4,
        OutOfRange = 5,
        Rejected = 6,
        Cancelled = 7,
    }
}