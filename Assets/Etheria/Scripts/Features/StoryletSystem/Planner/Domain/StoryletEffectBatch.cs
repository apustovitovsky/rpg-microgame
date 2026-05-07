using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletEffectBatch
    {
        public StoryletEffectBatch(IReadOnlyList<StoryletEffect> effects)
        {
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        public IReadOnlyList<StoryletEffect> Effects { get; }
    }
}
