namespace Etheria.Features.HWFC
{
    public readonly struct BitMask256 : System.IEquatable<BitMask256>
    {
        public const int Capacity = 256;

        public static readonly BitMask256 Empty = new(0, 0, 0, 0);

        private readonly ulong _bits0;
        private readonly ulong _bits1;
        private readonly ulong _bits2;
        private readonly ulong _bits3;

        public BitMask256(ulong bits0, ulong bits1 = 0, ulong bits2 = 0, ulong bits3 = 0)
        {
            _bits0 = bits0;
            _bits1 = bits1;
            _bits2 = bits2;
            _bits3 = bits3;
        }

        public int Count =>
            PopCount(_bits0) +
            PopCount(_bits1) +
            PopCount(_bits2) +
            PopCount(_bits3);

        public static BitMask256 FromIndex(int index)
        {
            var wordIndex = index >> 6;
            var bit = 1UL << (index & 63);

            return wordIndex switch
            {
                0 => new BitMask256(bit),
                1 => new BitMask256(0, bit),
                2 => new BitMask256(0, 0, bit),
                3 => new BitMask256(0, 0, 0, bit),
                _ => throw new System.ArgumentOutOfRangeException(nameof(index))
            };
        }

        public bool IsEmpty =>
            _bits0 == 0 &&
            _bits1 == 0 &&
            _bits2 == 0 &&
            _bits3 == 0;

        public bool ContainsIndex(int index)
        {
            if ((uint)index >= Capacity)
                return false;

            ulong bit = 1UL << (index & 63);

            return (index >> 6) switch
            {
                0 => (_bits0 & bit) != 0,
                1 => (_bits1 & bit) != 0,
                2 => (_bits2 & bit) != 0,
                3 => (_bits3 & bit) != 0,
                _ => false
            };
        }

        public bool ContainsAll(BitMask256 other)
        {
            return (_bits0 & other._bits0) == other._bits0 &&
                   (_bits1 & other._bits1) == other._bits1 &&
                   (_bits2 & other._bits2) == other._bits2 &&
                   (_bits3 & other._bits3) == other._bits3;
        }

        public bool Overlaps(BitMask256 other)
        {
            return (_bits0 & other._bits0) != 0 ||
                   (_bits1 & other._bits1) != 0 ||
                   (_bits2 & other._bits2) != 0 ||
                   (_bits3 & other._bits3) != 0;
        }

        public bool Excludes(BitMask256 other)
        {
            return !Overlaps(other);
        }

        public BitMask256 With(BitMask256 other)
        {
            return this | other;
        }

        public BitMask256 Without(BitMask256 other)
        {
            return this & ~other;
        }

        public BitMask256 InvertedWithin(BitMask256 universe)
        {
            return ~this & universe;
        }

        public bool Equals(BitMask256 other)
        {
            return _bits0 == other._bits0 &&
                   _bits1 == other._bits1 &&
                   _bits2 == other._bits2 &&
                   _bits3 == other._bits3;
        }

        public override bool Equals(object obj)
        {
            return obj is BitMask256 other && Equals(other);
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

        public static BitMask256 operator |(BitMask256 left, BitMask256 right)
        {
            return new BitMask256(
                left._bits0 | right._bits0,
                left._bits1 | right._bits1,
                left._bits2 | right._bits2,
                left._bits3 | right._bits3);
        }

        public static BitMask256 operator &(BitMask256 left, BitMask256 right)
        {
            return new BitMask256(
                left._bits0 & right._bits0,
                left._bits1 & right._bits1,
                left._bits2 & right._bits2,
                left._bits3 & right._bits3);
        }

        public static BitMask256 operator ~(BitMask256 value)
        {
            return new BitMask256(
                ~value._bits0,
                ~value._bits1,
                ~value._bits2,
                ~value._bits3);
        }

        public static bool operator ==(BitMask256 left, BitMask256 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BitMask256 left, BitMask256 right)
        {
            return !left.Equals(right);
        }

        private static int PopCount(ulong value)
        {
            value -= (value >> 1) & 0x5555555555555555UL;
            value = (value & 0x3333333333333333UL) + ((value >> 2) & 0x3333333333333333UL);
            return (int)((((value + (value >> 4)) & 0x0F0F0F0F0F0F0F0FUL) * 0x0101010101010101UL) >> 56);
        }
    }
}
