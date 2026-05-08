using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    [CreateAssetMenu(
        fileName = "StoryletDefinition",
        menuName = "Etheria/Features/StoryletSystem/Authoring/Storylet Definition")]
    public sealed class StoryletDefinitionSO : ScriptableObject
    {
        [field: SerializeField]
        public string StoryletId { get; private set; }

        [SerializeField]
        private TagCollectionAuthoring _storyletTags;

        [SerializeField]
        private TagRequirementAuthoring[] _preconditions;

        [SerializeField]
        private RoleAuthoring[] _roles;

        [SerializeField]
        private RepeatabilityPolicyAuthoring _repeatability;

        [SerializeField]
        private SaliencePolicyAuthoring _salience;

        // Legacy read-only compatibility for old storylet-global score bonuses.
        [SerializeField]
        [HideInInspector]
        private ScoreBonusAuthoring[] _legacyScoreBonuses;

        public TagCollectionAuthoring StoryletTags => _storyletTags;
        public IReadOnlyList<TagRequirementAuthoring> Preconditions => _preconditions;
        public IReadOnlyList<RoleAuthoring> Roles => _roles;
        public RepeatabilityPolicyAuthoring Repeatability => _repeatability;
        public SaliencePolicyAuthoring Salience => _salience;
    }
}
