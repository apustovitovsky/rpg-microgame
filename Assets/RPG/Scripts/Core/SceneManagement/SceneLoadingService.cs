using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace RPG.Core
{
    public sealed class SceneLoadingService : ISceneLoadingService
    {
        public async UniTask<LifetimeScope> LoadSceneAdditiveAsync(string sceneName, LifetimeScope parentScope)
        {
            using (LifetimeScope.EnqueueParent(parentScope))
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask();
            }
            return LifetimeScope.Find<LifetimeScope>(SceneManager.GetSceneByName(sceneName));
        }

        public async UniTask LoadSceneAdditiveAsync(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public async UniTask<LifetimeScope> LoadSceneAsync(string sceneName, LifetimeScope parentScope)
        {
            using (LifetimeScope.EnqueueParent(parentScope))
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single).ToUniTask();
            }
            return LifetimeScope.Find<LifetimeScope>(SceneManager.GetSceneByName(sceneName));
        }
    }
}