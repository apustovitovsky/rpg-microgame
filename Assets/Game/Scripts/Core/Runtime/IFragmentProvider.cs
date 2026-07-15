namespace Game.Core
{
    public interface IFragmentProvider
    {
        bool TryGetFragment<TFragment>(
            out TFragment fragment)
            where TFragment : class;
    }
}