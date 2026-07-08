using Game.Core;
using Game.Interaction;
using Game.Pickup;
using Game.World;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "PickupConfigurator",
        menuName = "Game/Gameplay/Pickup Configurator")]
    public sealed class PickupConfiguratorSO : BuildConfigurator
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<WorldRegistry<IWorldPickup>>(Lifetime.Singleton)
                .As<IWorldRegistry<IWorldPickup>>();

            builder.Register<WorldPickupService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterBuildCallback(container =>
            {
                var world = container.Resolve<IWorldManager>();
                var worldObjects = container.Resolve<IWorldRegistry<IWorldObject>>();
                var pickupsRegistry = container.Resolve<IWorldRegistry<IWorldPickup>>();
                var interactions = container.Resolve<IWorldRegistry<IInteractable>>();

                var pickups = FindObjectsByType<WorldPickup>(
                    FindObjectsInactive.Exclude);

                for (var i = 0; i < pickups.Length; i++)
                {
                    var pickup = pickups[i];

                    if (pickup == null)
                        continue;

                    var worldId = CreateWorldId(
                        pickup,
                        i + 1);

                    pickup.Initialize(worldId);

                    if (pickup.TryGetComponent<PickupInteract>(out var interaction))
                        container.Inject(interaction);

                    var worldObject = new WorldObject(
                        worldId,
                        pickup.gameObject);

                    var lifetime = new CompositeRegistration();
                    lifetime.Add(worldObjects.Register(worldId, worldObject));
                    lifetime.Add(pickupsRegistry.Register(worldId, pickup));

                    if (interaction != null)
                        lifetime.Add(interactions.Register(worldId, interaction));

                    if (!world.Track(worldObject, lifetime))
                    {
                        Debug.LogWarning(
                            $"Pickup '{worldId}' was built but could not be tracked.",
                            pickup);
                    }
                }
            });
        }

        private static WorldId CreateWorldId(
            WorldPickup pickup,
            int index)
        {
            var prefix = !string.IsNullOrWhiteSpace(pickup.DisplayName)
                ? pickup.DisplayName
                : pickup.gameObject.name;

            return new WorldId(
                $"{NormalizeWorldIdPrefix(prefix)}_{index:0000}");
        }

        private static string NormalizeWorldIdPrefix(string value)
        {
            value = value?.Trim().ToLowerInvariant() ?? "pickup";

            if (string.IsNullOrWhiteSpace(value))
                return "pickup";

            var chars = value.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }
    }
}