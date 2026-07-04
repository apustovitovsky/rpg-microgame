using VContainer;
using UnityEngine;

namespace Game.AI
{
    [DisallowMultipleComponent]
    public sealed class NavMeshPlannerEndpoint : MonoBehaviour
    {
        public INavMeshPlanner Planner { get; private set; }

        [Inject]
        public void Construct(INavMeshPlanner planner)
        {
            Planner = planner;
        }
    }
}