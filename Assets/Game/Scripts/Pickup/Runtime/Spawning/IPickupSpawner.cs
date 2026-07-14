namespace Game.Pickup
{
    public interface IPickupSpawner
    {
        PickupInstance Spawn(PickupSpawnRequest request);
    }
}