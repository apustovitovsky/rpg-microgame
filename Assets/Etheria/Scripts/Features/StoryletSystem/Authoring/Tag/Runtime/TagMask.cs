namespace Etheria.Features.StoryletSystem.Authoring
{
    public readonly struct TagMask : System.IEquatable<TagMask>
    {
        public const int Capacity = 256;

        public static readonly TagMask Empty = new(0, 0, 0, 0);

        private readonly ulong _bits0;
        private readonly ulong _bits1;
        private readonly ulong _bits2;
        private readonly ulong _bits3;

        public TagMask(ulong bits0, ulong bits1 = 0, ulong bits2 = 0, ulong bits3 = 0)
        {
            _bits0 = bits0;
            _bits1 = bits1;
            _bits2 = bits2;
            _bits3 = bits3;
        }

        public static TagMask FromIndex(int index)
        {
            var wordIndex = index >> 6;
            var bit = 1UL << (index & 63);

            return wordIndex switch
            {
                0 => new TagMask(bit),
                1 => new TagMask(0, bit),
                2 => new TagMask(0, 0, bit),
                3 => new TagMask(0, 0, 0, bit),
                _ => throw new System.ArgumentOutOfRangeException(nameof(index))
            };
        }

        public static TagMask FromId(TagId tagId)
        {
            return FromIndex(tagId.Index);
        }

        public bool IsEmpty =>
            _bits0 == 0 &&
            _bits1 == 0 &&
            _bits2 == 0 &&
            _bits3 == 0;

        public bool ContainsAll(TagMask other)
        {
            return (_bits0 & other._bits0) == other._bits0 &&
                   (_bits1 & other._bits1) == other._bits1 &&
                   (_bits2 & other._bits2) == other._bits2 &&
                   (_bits3 & other._bits3) == other._bits3;
        }

        public bool Overlaps(TagMask other)
        {
            return (_bits0 & other._bits0) != 0 ||
                   (_bits1 & other._bits1) != 0 ||
                   (_bits2 & other._bits2) != 0 ||
                   (_bits3 & other._bits3) != 0;
        }

        public bool Excludes(TagMask other)
        {
            return !Overlaps(other);
        }

        public TagMask With(TagMask other)
        {
            return this | other;
        }

        public TagMask Without(TagMask other)
        {
            return this & ~other;
        }

        public bool Equals(TagMask other)
        {
            return _bits0 == other._bits0 &&
                   _bits1 == other._bits1 &&
                   _bits2 == other._bits2 &&
                   _bits3 == other._bits3;
        }

        public override bool Equals(object obj)
        {
            return obj is TagMask other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = _bits0.GetHashCode();
                hash = (hash * 397) ^ _bits1.GetHashCode();
                hash = (hash * 397) ^ _bits2.GetHashCode();
                hash = (hash * 397) ^ _bits3.GetHashCode();
                return hash;
            }
        }

        public static TagMask operator |(TagMask left, TagMask right)
        {
            return new TagMask(
                left._bits0 | right._bits0,
                left._bits1 | right._bits1,
                left._bits2 | right._bits2,
                left._bits3 | right._bits3);
        }

        public static TagMask operator &(TagMask left, TagMask right)
        {
            return new TagMask(
                left._bits0 & right._bits0,
                left._bits1 & right._bits1,
                left._bits2 & right._bits2,
                left._bits3 & right._bits3);
        }

        public static TagMask operator ~(TagMask value)
        {
            return new TagMask(
                ~value._bits0,
                ~value._bits1,
                ~value._bits2,
                ~value._bits3);
        }

        public static bool operator ==(TagMask left, TagMask right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TagMask left, TagMask right)
        {
            return !left.Equals(right);
        }
    }
}
