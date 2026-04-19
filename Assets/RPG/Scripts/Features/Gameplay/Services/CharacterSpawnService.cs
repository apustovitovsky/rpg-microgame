using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class CharacterSpawnService : ICharacterSpawnService
    {
        private readonly GameplayConfigSO _gameplaySettings;
        private readonly ICharacterFactory _factory;
        private readonly IPossessionService _possessionService;

        public CharacterSpawnService(
            ICharacterFactory factory,
            GameplayConfigSO gameplaySettings,
            IPossessionService possessionService)
        {
            _factory = factory;
            _gameplaySettings = gameplaySettings;
            _possessionService = possessionService;
        }

        public LifetimeScope SpawnPlayer(Vector3 position)
        {
            return SpawnAndPossess(_gameplaySettings.PlayerPrefab, position);
        }

        public LifetimeScope SpawnTurret(Vector3 position)
        {
            return SpawnAndPossess(_gameplaySettings.TurretPrefab, position);
        }

        public LifetimeScope SpawnDrone(Vector3 position)
        {
            return SpawnAndPossess(_gameplaySettings.DronePrefab, position);
        }

        private LifetimeScope SpawnAndPossess(LifetimeScope pawnPrefab, Vector3 position)
        {
            var scope = _factory.Create(pawnPrefab, position);
            _possessionService.Possess(scope);
            return scope;
        }
    }
}
