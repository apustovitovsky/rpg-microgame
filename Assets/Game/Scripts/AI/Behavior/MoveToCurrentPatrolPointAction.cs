using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Game.AI.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Move To Current Patrol Point",
        story: "[Self] moves to patrol point [PatrolIndex] from [PatrolPoints]",
        category: "Game/AI/Navigation",
        id: "0d99690f0dc0487a8b3d9942da1187b1")]
    public partial class MoveToCurrentPatrolPointAction :
        Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<List<GameObject>> PatrolPoints;
        [SerializeReference] public BlackboardVariable<int> PatrolIndex;

        private NavMeshPlannerEndpoint _navigation;
        private GameObject _point;

        protected override Status OnStart()
        {
            if (Self?.Value == null)
                return Status.Failure;

            if (!TryGetCurrentPoint(out _point))
                return Status.Failure;

            _navigation = Self.Value.GetComponentInParent<NavMeshPlannerEndpoint>();

            if (_navigation == null || _navigation.Planner == null)
                return Status.Failure;

            _navigation.Planner.MoveTo(_point.transform.position);

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (_navigation == null ||
                _navigation.Planner == null ||
                _point == null)
            {
                return Status.Failure;
            }

            return _navigation.Planner.HasArrived
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