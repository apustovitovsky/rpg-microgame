using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace RPG.Core
{
    public sealed class SceneLoadingService : ISceneLoadingService
    {
        public async UniTask LoadSceneAdditiveAsync(string sceneName, LifetimeScope parentScope)
        {
            using (LifetimeScope.EnqueueParent(parentScope))
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask();
            }
        }

        public async UniTask LoadSceneAdditiveAsync(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public async UniTask LoadSceneAsync(string sceneName, LifetimeScope parentScope)
        {
            using (LifetimeScope.EnqueueParent(parentScope))
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single).ToUniTask();
            }
        }
    }
}
