using UnityEngine;
using VContainer.Unity;

namespace RPG.Gameplay
{
    public sealed class ActorSpawnService : IActorSpawnService
    {
        private readonly IActorFactory _factory;
        private readonly IPossessionService _possessionService;

        public ActorSpawnService(
            IActorFactory factory,
            IPossessionService possessionService)
        {
            _factory = factory;
            _possessionService = possessionService;
        }

        public LifetimeScope SpawnAndPossess(LifetimeScope prefab, Vector3 position, Quaternion rotation = default)
        {
            var actorId = Spawn(prefab, position, rotation);
            _possessionService.Possess(actorId);
            return actorId;
        }

        public LifetimeScope Spawn(LifetimeScope prefab, Vector3 position, Quaternion rotation = default)
        {
            return _factory.Create(prefab, position, rotation);
        }
    }
}
