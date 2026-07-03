using Game.Core;

using UnityEngine;
using VContainer;


namespace Game.Player
{
    [CreateAssetMenu(
        fileName = "PlayerConfiguration",
        menuName = "Game/Player/Player Configuration")]
    public sealed class PlayerConfigurationSO : BuildConfigurationSO
    {

        public override void Install(IContainerBuilder builder)
        {
            builder.Register<PlayerInputService>(Lifetime.Singleton)
                .AsImplementedInterfaces();

            builder.Register<PlayerActorSpawner>(Lifetime.Singleton)
                .AsImplementedInterfaces();


        }
    }
}