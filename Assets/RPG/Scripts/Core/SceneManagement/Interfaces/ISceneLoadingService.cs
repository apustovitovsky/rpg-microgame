using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine.SceneManagement;
using VContainer.Unity;


namespace RPG.Core
{
    public interface ISceneLoadingService
    {
        UniTask<LifetimeScope> LoadSceneAsync(string sceneName, LifetimeScope parentScope);
        UniTask<LifetimeScope> LoadSceneAdditiveAsync(string sceneName, LifetimeScope parentScope);
        UniTask LoadSceneAdditiveAsync(string sceneName);
    }
}