using Etheria.Core;
using UnityEngine;

namespace Etheria.Game
{
    public sealed class GameTimeService : ITimeProvider
    {
        public float DeltaTime => Time.deltaTime;
    }
}