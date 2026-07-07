namespace Game.Pickup
{
    public enum PickupResult
    {
        Succeeded = 0,
        HandlerProviderNotFound = 1,
        PickupNotFound = 2,
        CannotBeCollected = 3,
        EffectHandlerNotFound = 4,
        EffectCannotApply = 5,
        Failed = 6
    }
}
