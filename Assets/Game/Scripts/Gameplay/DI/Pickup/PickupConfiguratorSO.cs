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
            builder.Register<WorldPickupService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.RegisterBuildCallback(container =>
            {
                var world = container.Resolve<IWorldManager>();

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

                    var builder = new WorldObjectBuilder()
                        .Add<IWorldPickup>(pickup);

                    if (interaction != null)
                        builder.Add<IInteractable>(interaction);

                    var worldObject = builder.Build(
                        worldId,
                        pickup.gameObject);

                    if (!world.Register(worldObject))
                    {
                        Debug.LogWarning(
                            $"Pickup '{worldId}' was built but could not be registered.",
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