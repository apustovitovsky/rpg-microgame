namespace Game.Interaction
{
    public enum InteractionResult
    {
        Succeeded = 0,
        Rejected = 1,
        Cancelled = 2,
        InteractorNotFound = 3,
        InteractableNotFound = 4,
        OutOfRange = 5,
    }
}