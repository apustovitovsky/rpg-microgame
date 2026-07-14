using Game.Core;
using Game.Loot;
using Game.Targeting;
using UnityEngine;
using VContainer;

namespace Game.Gameplay
{
    [CreateAssetMenu(
        fileName = "LootActorModuleBuilder",
        menuName = "Game/Gameplay/Loot Actor Module Builder")]
    public sealed class LootActorModuleBuilder :
        ModuleBuilder
    {
        public override void Install(
            IContainerBuilder builder)
        {
            builder.RegisterComponentInModuleRoot<Targetable>()
                .AsSelf()
                .AsImplementedInterfaces();

            builder.RegisterComponentInModuleRoot<LootInteractable>()
                .AsImplementedInterfaces();
        }
    }
}