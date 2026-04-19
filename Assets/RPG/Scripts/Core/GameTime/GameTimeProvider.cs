using UnityEngine;

namespace RPG.Core
{
    public sealed class GameTimeService : ITimeProvider
    {
        public float DeltaTime => Time.deltaTime;
    }
}