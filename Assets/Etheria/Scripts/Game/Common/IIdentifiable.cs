namespace Etheria.Game.Common
{
    public interface IIdentifiable<out TId>
    {
        TId Id { get; }
    }
}
