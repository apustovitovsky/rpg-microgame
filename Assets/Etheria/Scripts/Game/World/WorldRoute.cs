using UnityEngine;

namespace Etheria.Game.World
{
    public sealed class WorldRoute : MonoBehaviour
    {
        [SerializeField] private string _id;
        [SerializeField] private WorldLocation[] _nodes;

        public string Id => _id;
        public WorldLocation[] Nodes => _nodes;
    }
}