namespace Etheria.Features.NarrativeGeneration
{
    public readonly struct TagSet : System.IEquatable<TagSet>
    {
        public static readonly TagSet None = new(0, 0, 0, 0);

        private readonly ulong _word0;
        private readonly ulong _word1;
        private readonly ulong _word2;
        private readonly ulong _word3;

        public TagSet(ulong word0, ulong word1 = 0, ulong word2 = 0, ulong word3 = 0)
        {
            _word0 = word0;
            _word1 = word1;
            _word2 = word2;
            _word3 = word3;
        }

        public bool Equals(TagSet other)
        {
            return _word0 == other._word0 &&
                   _word1 == other._word1 &&
                   _word2 == other._word2 &&
                   _word3 == other._word3;
        }

        public override bool Equals(object obj)
        {
            return obj is TagSet other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = _word0.GetHashCode();
                hash = (hash * 397) ^ _word1.GetHashCode();
                hash = (hash * 397) ^ _word2.GetHashCode();
                hash = (hash * 397) ^ _word3.GetHashCode();
                return hash;
            }
        }

        public bool ContainsAll(TagSet other)
        {
            return (_word0 & other._word0) == other._word0 &&
                   (_word1 & other._word1) == other._word1 &&
                   (_word2 & other._word2) == other._word2 &&
                   (_word3 & other._word3) == other._word3;
        }

        public bool ContainsAny(TagSet other)
        {
            return (_word0 & other._word0) != 0 ||
                   (_word1 & other._word1) != 0 ||
                   (_word2 & other._word2) != 0 ||
                   (_word3 & other._word3) != 0;
        }

        public static TagSet operator |(TagSet left, TagSet right)
        {
            return new TagSet(
                left._word0 | right._word0,
                left._word1 | right._word1,
                left._word2 | right._word2,
                left._word3 | right._word3);
        }

        public static TagSet operator &(TagSet left, TagSet right)
        {
            return new TagSet(
                left._word0 & right._word0,
                left._word1 & right._word1,
                left._word2 & right._word2,
                left._word3 & right._word3);
        }

        public static TagSet operator ~(TagSet value)
        {
            return new TagSet(
                ~value._word0,
                ~value._word1,
                ~value._word2,
                ~value._word3);
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

    public static class PrototypeTags
    {
        public static readonly TagSet None = TagSet.None;

        // Entity tags
        public static readonly TagSet BanditHideout = FromBit(0);
        public static readonly TagSet Farm = FromBit(1);
        public static readonly TagSet Village = FromBit(2);
        public static readonly TagSet Destroyed = FromBit(3);

        // Relation tags
        public static readonly TagSet Threatens = FromBit(16);
        public static readonly TagSet BelongsTo = FromBit(17);

        private static TagSet FromBit(int bitIndex)
        {
            if (bitIndex < 0 || bitIndex >= 256)
            {
                throw new System.ArgumentOutOfRangeException(nameof(bitIndex));
            }

            var wordIndex = bitIndex / 64;
            var bitOffset = bitIndex % 64;
            var bit = 1ul << bitOffset;

            return wordIndex switch
            {
                0 => new TagSet(bit, 0, 0, 0),
                1 => new TagSet(0, bit, 0, 0),
                2 => new TagSet(0, 0, bit, 0),
                3 => new TagSet(0, 0, 0, bit),
                _ => TagSet.None
            };
        }
    }
}
