using Cysharp.Threading.Tasks;
using Etheria.Core;
using Etheria.Core.DI;
using UnityEngine;

namespace Etheria.Game
{
    public sealed class SessionNavigator : ISessionNavigator
    {
        private readonly SceneCatalogSO _sceneCatalog;
        private readonly IGameNavigator _gameNavigator;

        public SessionNavigator(
            SceneCatalogSO sceneCatalog,
            IGameNavigator gameNavigator)
        {
            _sceneCatalog = sceneCatalog;
            _gameNavigator = gameNavigator;
        }

        public UniTask LoadMainMenuScene()
        {
            return LoadSceneAsync("MainMenu");
        }

        public UniTask LoadRPGScene()
        {
            return LoadSceneAsync("Etheria");
        }

        public UniTask LoadFPSScene()
        {
            return LoadSceneAsync("FPS");
        }

        public UniTask LoadSyntyScene()
        {
            return LoadSceneAsync("Synty");
        }

        private UniTask LoadSceneAsync(string entryName)
        {
            if (!_sceneCatalog.TryGet(entryName, out var entry) || entry == null)
            {
                Debug.LogWarning($"Scene catalog entry '{entryName}' is not configured.");
                return UniTask.CompletedTask;
            }

            return _gameNavigator.LoadScene(entry, showLoading: true);
        }
    }
}
