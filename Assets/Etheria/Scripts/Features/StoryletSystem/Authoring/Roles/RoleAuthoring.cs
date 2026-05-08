using System;
using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Features.StoryletSystem.Authoring
{
    [Serializable]
    public sealed class RoleAuthoring
    {
        [SerializeField]
        private string _roleName;

        [SerializeField]
        private TagRequirementAuthoring[] _tagRequirements;

        [SerializeField]
        private RelationRequirementAuthoring[] _relationRequirements;

        [SerializeField]
        private ScoreBonusAuthoring[] _scoreBonuses;

        public string RoleName => _roleName;
        public IReadOnlyList<TagRequirementAuthoring> TagRequirements => _tagRequirements;
        public IReadOnlyList<RelationRequirementAuthoring> RelationRequirements => _relationRequirements;
        public IReadOnlyList<ScoreBonusAuthoring> ScoreBonuses => _scoreBonuses;
    }
}
