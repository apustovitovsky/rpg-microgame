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
    public sealed class PickupConfiguratorSO : BuildConfiguratorSO
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterBuildCallback(container =>
            {
                var registry = container.Resolve<IWorldObjectRegistryWriter>();

                var pickups = FindObjectsByType<PickupComponent>(
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

                    var interaction = pickup.GetComponent<PickupInteractible>();

                    var worldPickup = new WorldPickup(
                        worldId,
                        pickup.DisplayName,
                        pickup.Root,
                        pickup,
                        interaction);

                    registry.Register(worldPickup);
                }
            });
        }

        private static WorldId CreateWorldId(
            PickupComponent pickup,
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