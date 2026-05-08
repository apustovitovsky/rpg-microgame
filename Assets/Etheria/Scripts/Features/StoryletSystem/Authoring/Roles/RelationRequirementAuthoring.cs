using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    [System.Serializable]
    public struct RelationRequirementAuthoring
    {
        [field: SerializeField]
        public string TargetRoleName { get; private set; }

        [field: SerializeField]
        public TagRequirementAuthoring Conditions { get; private set; }
    }
}
