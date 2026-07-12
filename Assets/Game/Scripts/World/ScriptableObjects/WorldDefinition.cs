using Game.Core;
using UnityEngine;

namespace Game.World
{
    public abstract class WorldDefinition<TInstance> :
        AssetDefinition<TInstance>
        where TInstance : class, IWorldInstance
    {
        [field: SerializeField]
        public GameObject Prefab { get; private set; }
    }
}