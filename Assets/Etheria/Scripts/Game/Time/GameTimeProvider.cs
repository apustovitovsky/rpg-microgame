using Etheria.Core;
using UnityEngine;

namespace Etheria.Game
{
    public sealed class GameTimeProvider : IGameTimeProvider
    {
        public float DeltaTime => Time.deltaTime;
    }
}