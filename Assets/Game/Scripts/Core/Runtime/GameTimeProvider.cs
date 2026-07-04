using UnityEngine;

namespace Game.Core
{
    public sealed class GameTimeProvider :
        IGameTimeProvider
    {
        public float DeltaTime =>
            Time.deltaTime;
    }
}