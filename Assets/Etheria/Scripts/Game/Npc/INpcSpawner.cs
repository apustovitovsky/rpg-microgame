using UnityEngine;

namespace Etheria.Game.Npc
{
    public interface INpcSpawner
    {
        GameObject Spawn(
            string npcId,
            Vector3 position,
            Quaternion rotation);
    }
}