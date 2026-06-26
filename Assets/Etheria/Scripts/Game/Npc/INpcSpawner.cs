using UnityEngine;

namespace Etheria.Game.Npc
{
    public interface INpcSpawner
    {
        public GameObject Spawn(
            string npcId,
            Transform transform);
    }
}