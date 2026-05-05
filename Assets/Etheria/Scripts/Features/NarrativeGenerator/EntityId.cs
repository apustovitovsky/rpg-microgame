namespace Etheria.Features.NarrativeGeneration
{
    public readonly struct EntityId
    {
        public readonly int Value;

        public EntityId(int value)
        {
            Value = value;
        }

        public bool IsValid => Value >= 0;

        public static EntityId Invalid => new(-1);
    }
}












