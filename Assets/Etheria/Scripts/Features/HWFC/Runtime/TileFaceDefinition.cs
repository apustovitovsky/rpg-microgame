using System;
using UnityEngine;


namespace Etheria.Features.HWFC
{
    [Serializable]
    public sealed class TileFaceDefinition
    {
        [SerializeField] private string _socketId = string.Empty;
        [SerializeField] private bool _walkable;
        [SerializeField] private bool _isBoundary;

        public string SocketId => _socketId;
        public bool Walkable => _walkable;
        public bool IsBoundary => _isBoundary;
    }
}
