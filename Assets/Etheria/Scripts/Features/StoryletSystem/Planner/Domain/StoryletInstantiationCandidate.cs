using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletInstantiationCandidate
    {
        public StoryletInstantiationCandidate(
            StoryletDefinition definition,
            Storylet legacyStorylet,
            IReadOnlyList<RoleAssignment> assignment,
            StoryletEffectBatch effectPreview,
            float instantiationQuality,
            StoryletSalienceEvaluation salience)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            LegacyStorylet = legacyStorylet ?? throw new ArgumentNullException(nameof(legacyStorylet));
            Assignment = assignment ?? throw new ArgumentNullException(nameof(assignment));
            EffectPreview = effectPreview ?? throw new ArgumentNullException(nameof(effectPreview));
            InstantiationQuality = instantiationQuality;
            Salience = salience ?? throw new ArgumentNullException(nameof(salience));
        }

        public StoryletDefinition Definition { get; }
        public Storylet LegacyStorylet { get; }
        public IReadOnlyList<RoleAssignment> Assignment { get; }
        public StoryletEffectBatch EffectPreview { get; }
        public float InstantiationQuality { get; }
        public StoryletSalienceEvaluation Salience { get; set; }
        public StoryletScoreBreakdown ScoreBreakdown { get; set; }
    }
}
