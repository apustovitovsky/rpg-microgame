namespace Etheria.Features.StoryletSystem
{
    public readonly struct TagSet : System.IEquatable<TagSet>
    {
        public const int Capacity = 256;

        public static readonly TagSet Empty = new(0, 0, 0, 0);

        private readonly ulong _bits0;
        private readonly ulong _bits1;
        private readonly ulong _bits2;
        private readonly ulong _bits3;

        public TagSet(ulong bits0, ulong bits1 = 0, ulong bits2 = 0, ulong bits3 = 0)
        {
            _bits0 = bits0;
            _bits1 = bits1;
            _bits2 = bits2;
            _bits3 = bits3;
        }

        public static TagSet FromIndex(int index)
        {
            var wordIndex = index >> 6;
            var bit = 1UL << (index & 63);

            return wordIndex switch
            {
                0 => new TagSet(bit),
                1 => new TagSet(0, bit),
                2 => new TagSet(0, 0, bit),
                3 => new TagSet(0, 0, 0, bit),
                _ => throw new System.ArgumentOutOfRangeException(nameof(index))
            };
        }

        public bool IsEmpty =>
            _bits0 == 0 &&
            _bits1 == 0 &&
            _bits2 == 0 &&
            _bits3 == 0;

        public bool ContainsAll(TagSet other)
        {
            return (_bits0 & other._bits0) == other._bits0 &&
                   (_bits1 & other._bits1) == other._bits1 &&
                   (_bits2 & other._bits2) == other._bits2 &&
                   (_bits3 & other._bits3) == other._bits3;
        }

        public bool Overlaps(TagSet other)
        {
            return (_bits0 & other._bits0) != 0 ||
                   (_bits1 & other._bits1) != 0 ||
                   (_bits2 & other._bits2) != 0 ||
                   (_bits3 & other._bits3) != 0;
        }

        public bool Excludes(TagSet other)
        {
            return !Overlaps(other);
        }

        public TagSet Without(TagSet other)
        {
            return this & ~other;
        }

        public bool Equals(TagSet other)
        {
            return _bits0 == other._bits0 &&
                   _bits1 == other._bits1 &&
                   _bits2 == other._bits2 &&
                   _bits3 == other._bits3;
        }

        public override bool Equals(object obj)
        {
            return obj is TagSet other && Equals(other);
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

        public static TagSet operator |(TagSet left, TagSet right)
        {
            return new TagSet(
                left._bits0 | right._bits0,
                left._bits1 | right._bits1,
                left._bits2 | right._bits2,
                left._bits3 | right._bits3);
        }

        public static TagSet operator &(TagSet left, TagSet right)
        {
            return new TagSet(
                left._bits0 & right._bits0,
                left._bits1 & right._bits1,
                left._bits2 & right._bits2,
                left._bits3 & right._bits3);
        }

        public static TagSet operator ~(TagSet value)
        {
            return new TagSet(
                ~value._bits0,
                ~value._bits1,
                ~value._bits2,
                ~value._bits3);
        }

        public static bool operator ==(TagSet left, TagSet right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TagSet left, TagSet right)
        {
            return !left.Equals(right);
        }
    }
}