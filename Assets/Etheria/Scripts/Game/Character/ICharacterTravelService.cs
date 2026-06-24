namespace Etheria.Game.Character
{
    public interface ICharacterTravelService
    {
        bool TrySend(
            string characterId,
            string locationId);

        bool TrySendRoute(
            string characterId,
            string routeId);
    }
}