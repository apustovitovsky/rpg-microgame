using Cysharp.Threading.Tasks;

namespace RPG.Core
{
    public interface ISceneInitiator
    {
        UniTask<bool> Ready { get; }
    }
}