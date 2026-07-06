using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Game.AI.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait At Current Patrol Point",
        story: "[Navigation] waits [WaitSeconds] seconds at patrol point [PatrolIndex] from [PatrolPoints]",
        category: "Game/AI/Navigation",
        id: "6b3d91b0f5064ed7b0e14fb12db2e6f8")]
    public partial class WaitAtCurrentPatrolPointAction :
        Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<NavMeshTravelEndpoint> Navigation;
        [SerializeReference] public BlackboardVariable<List<GameObject>> PatrolPoints;
        [SerializeReference] public BlackboardVariable<int> PatrolIndex;
        [SerializeReference] public BlackboardVariable<float> WaitSeconds = new(2f);

        private GameObject _point;
        private float _timer;

        protected override Status OnStart()
        {
            if (Navigation?.Value == null)
                return Status.Failure;

            if (!TryGetCurrentPoint(out _point))
                return Status.Failure;

            Navigation.Value.Stop();
            Navigation.Value.FaceDirection(_point.transform.forward);

            _timer = Mathf.Max(0f, WaitSeconds?.Value ?? 0f);

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Navigation?.Value == null || _point == null)
                return Status.Failure;

            if (!Navigation.Value.IsFacingComplete)
            {
                Navigation.Value.FaceDirection(_point.transform.forward);
            }

            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
                return Status.Running;
            }

            Navigation.Value.ClearFacing();
            Advance();

            return Status.Success;
        }

        protected override void OnEnd()
        {
            Navigation?.Value?.ClearFacing();
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

        private void Advance()
        {
            var points = PatrolPoints?.Value;

            if (points == null || points.Count == 0 || PatrolIndex == null)
                return;

            PatrolIndex.Value = (PatrolIndex.Value + 1) % points.Count;
        }
    }
}