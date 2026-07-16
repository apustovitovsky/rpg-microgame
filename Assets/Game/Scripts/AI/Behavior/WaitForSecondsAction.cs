using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace Game.AI.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Wait For Seconds",
        story: "waits [Duration] seconds",
        category: "Game/AI",
        id: "b09f50f20ea34a66925f8ced6306c21d")]
    public partial class WaitForSecondsAction :
        Unity.Behavior.Action
    {
        [SerializeReference]
        public BlackboardVariable<float> Duration =
            new(2f);

        private float _remainingSeconds;

        protected override Status OnStart()
        {
            _remainingSeconds = Mathf.Max(
                0f,
                Duration?.Value ?? 0f);

            return _remainingSeconds <= 0f
                ? Status.Success
                : Status.Running;
        }

        protected override Status OnUpdate()
        {
            _remainingSeconds -= Time.deltaTime;

            return _remainingSeconds <= 0f
                ? Status.Success
                : Status.Running;
        }
    }
}