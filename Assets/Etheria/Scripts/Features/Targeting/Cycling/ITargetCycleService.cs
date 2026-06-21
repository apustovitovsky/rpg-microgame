namespace Etheria.Features.Targeting
{
    public interface ITargetCycleService
    {
        TargetCycleResult Cycle(ITargetable currentTarget, int direction);
    }
}

