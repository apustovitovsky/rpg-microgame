namespace Etheria.Features.StoryletSystem
{
    public readonly struct AttributeSet
    {
        private readonly float[] _values;

        public AttributeSet(float[] values)
        {
            _values = values;
        }

        public bool IsEmpty => _values == null || _values.Length == 0;

        public bool Has(AttributeId attributeId)
        {
            var index = attributeId.Value;
            return _values != null && index >= 0 && index < _values.Length;
        }

        public bool TryGet(AttributeId attributeId, out float value)
        {
            if (!Has(attributeId))
            {
                value = default;
                return false;
            }

            value = _values[attributeId.Value];
            return true;
        }

        public float GetOrDefault(AttributeId attributeId, float defaultValue = 0f)
        {
            return TryGet(attributeId, out var value)
                ? value
                : defaultValue;
        }

        public bool Matches(AttributeRequirement[] requirements)
        {
            if (requirements == null || requirements.Length == 0)
            {
                return true;
            }

            foreach (var requirement in requirements)
            {
                if (!Matches(requirement))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Matches(AttributeRequirement requirement)
        {
            var value = GetOrDefault(requirement.AttributeId);

            if (requirement.HasMinValue && value < requirement.MinValue)
            {
                return false;
            }

            if (requirement.HasMaxValue && value > requirement.MaxValue)
            {
                return false;
            }

            return true;
        }
    }
}
