using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Game.AI.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Move To Current Patrol Point",
        story: "[Navigation] moves to patrol point [PatrolIndex] from [PatrolPoints]",
        category: "Game/AI/Navigation",
        id: "0d99690f0dc0487a8b3d9942da1187b1")]
    public partial class MoveToCurrentPatrolPointAction :
        Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<NavMeshTravelEndpoint> Navigation;
        [SerializeReference] public BlackboardVariable<List<GameObject>> PatrolPoints;
        [SerializeReference] public BlackboardVariable<int> PatrolIndex;

        private GameObject _point;


        protected override Status OnStart()
        {
            if (Navigation?.Value == null)
                return Status.Failure;

            if (!TryGetCurrentPoint(out _point))
                return Status.Failure;

            Navigation.Value.MoveToPositionAsync(
                _point.transform.position,
                CancellationToken.None).Forget();

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Navigation?.Value == null || _point == null)
                return Status.Failure;

            return Navigation.Value.HasArrived
                ? Status.Success
                : Status.Running;
        }

        protected override void OnEnd()
        {
        }

        private bool TryGetCurrentPoint(out GameObject point)
        {
            point = null;

            var points = PatrolPoints?.Value;

            if (points == null || points.Count == 0)
                return false;

            var index = Mathf.Clamp(
                PatrolIndex?.Value ?? 0,
                0,
                points.Count - 1);

            point = points[index];

            return point != null;
        }
    }
}