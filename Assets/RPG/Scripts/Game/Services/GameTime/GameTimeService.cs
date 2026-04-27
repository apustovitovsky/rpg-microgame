using RPG.Core;
using UnityEngine;

namespace RPG.Game
{
    public sealed class GameTimeService : ITimeProvider
    {
        public float DeltaTime => Time.deltaTime;
    }
}