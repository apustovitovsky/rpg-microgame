using UnityEngine.Events;

namespace Unity.FPS.Game
{
    public class GlobalEventManager
    {
        public static UnityEvent<int> EnemySlainAction = new();

        public static void OnEnemySlain(int count)
        {
            EnemySlainAction.Invoke(count);
        }
    }
}



