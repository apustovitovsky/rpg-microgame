using System;
using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    [Serializable]
    public struct ModuleSet : IEquatable<ModuleSet>
    {
        public const int Capacity = 256;

        [SerializeField] private ulong _bits0;
        [SerializeField] private ulong _bits1;
        [SerializeField] private ulong _bits2;
        [SerializeField] private ulong _bits3;
        [SerializeField] private int _moduleCount;

        public ModuleSet(int moduleCount, bool filled)
        {
            if (moduleCount < 0 || moduleCount > Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(moduleCount));
            }

            _bits0 = 0UL;
            _bits1 = 0UL;
            _bits2 = 0UL;
            _bits3 = 0UL;
            _moduleCount = moduleCount;

            if (filled)
            {
                FillUsedBits();
            }
        }

        public int ModuleCount
        {
            get { return _moduleCount; }
        }

        public bool IsEmpty
        {
            get
            {
                return _bits0 == 0UL &&
                       _bits1 == 0UL &&
                       _bits2 == 0UL &&
                       _bits3 == 0UL;
            }
        }

        public static ModuleSet Empty(int moduleCount)
        {
            return new ModuleSet(moduleCount, false);
        }

        public static ModuleSet Filled(int moduleCount)
        {
            return new ModuleSet(moduleCount, true);
        }

        public static ModuleSet Single(int moduleCount, int moduleIndex)
        {
            var result = Empty(moduleCount);
            result.Add(moduleIndex);
            return result;
        }

        public bool Contains(int moduleIndex)
        {
            ValidateIndex(moduleIndex);
            return (GetWord(moduleIndex) & GetBit(moduleIndex)) != 0UL;
        }

        public void Add(int moduleIndex)
        {
            ValidateIndex(moduleIndex);
            SetWord(moduleIndex, GetWord(moduleIndex) | GetBit(moduleIndex));
        }

        public void Remove(int moduleIndex)
        {
            ValidateIndex(moduleIndex);
            SetWord(moduleIndex, GetWord(moduleIndex) & ~GetBit(moduleIndex));
        }

        public void Clear()
        {
            _bits0 = 0UL;
            _bits1 = 0UL;
            _bits2 = 0UL;
            _bits3 = 0UL;
        }

        public int CountBits()
        {
            return CountBits(_bits0) + CountBits(_bits1) + CountBits(_bits2) + CountBits(_bits3);
        }

        public bool Overlaps(ModuleSet other)
        {
            EnsureCompatible(other);
            return (_bits0 & other._bits0) != 0UL ||
                   (_bits1 & other._bits1) != 0UL ||
                   (_bits2 & other._bits2) != 0UL ||
                   (_bits3 & other._bits3) != 0UL;
        }

        public void IntersectWith(ModuleSet other)
        {
            EnsureCompatible(other);
            _bits0 = _bits0 & other._bits0;
            _bits1 = _bits1 & other._bits1;
            _bits2 = _bits2 & other._bits2;
            _bits3 = _bits3 & other._bits3;
        }

        public void UnionWith(ModuleSet other)
        {
            EnsureCompatible(other);
            _bits0 = _bits0 | other._bits0;
            _bits1 = _bits1 | other._bits1;
            _bits2 = _bits2 | other._bits2;
            _bits3 = _bits3 | other._bits3;
        }

        public bool Equals(ModuleSet other)
        {
            return _bits0 == other._bits0 &&
                   _bits1 == other._bits1 &&
                   _bits2 == other._bits2 &&
                   _bits3 == other._bits3 &&
                   _moduleCount == other._moduleCount;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ModuleSet))
            {
                return false;
            }

            return Equals((ModuleSet)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = _bits0.GetHashCode();
                hash = (hash * 397) ^ _bits1.GetHashCode();
                hash = (hash * 397) ^ _bits2.GetHashCode();
                hash = (hash * 397) ^ _bits3.GetHashCode();
                hash = (hash * 397) ^ _moduleCount;
                return hash;
            }
        }

        public static ModuleSet operator &(ModuleSet left, ModuleSet right)
        {
            left.IntersectWith(right);
            return left;
        }

        public static ModuleSet operator |(ModuleSet left, ModuleSet right)
        {
            left.UnionWith(right);
            return left;
        }

        public static bool operator ==(ModuleSet left, ModuleSet right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModuleSet left, ModuleSet right)
        {
            return !left.Equals(right);
        }

        private void FillUsedBits()
        {
            var remaining = _moduleCount;

            _bits0 = FillWord(ref remaining);
            _bits1 = FillWord(ref remaining);
            _bits2 = FillWord(ref remaining);
            _bits3 = FillWord(ref remaining);
        }

        private static ulong FillWord(ref int remaining)
        {
            ulong value;

            if (remaining <= 0)
            {
                return 0UL;
            }

            if (remaining >= 64)
            {
                remaining -= 64;
                return ulong.MaxValue;
            }

            value = (1UL << remaining) - 1UL;
            remaining = 0;
            return value;
        }

        private static int CountBits(ulong value)
        {
            var count = 0;

            while (value != 0UL)
            {
                value &= value - 1UL;
                count++;
            }

            return count;
        }

        private void EnsureCompatible(ModuleSet other)
        {
            if (_moduleCount != other._moduleCount)
            {
                throw new InvalidOperationException("Cannot combine module sets with different sizes.");
            }
        }

        private void ValidateIndex(int moduleIndex)
        {
            if (moduleIndex < 0 || moduleIndex >= _moduleCount)
            {
                throw new ArgumentOutOfRangeException(nameof(moduleIndex));
            }
        }

        private static ulong GetBit(int moduleIndex)
        {
            return 1UL << (moduleIndex & 63);
        }

        private ulong GetWord(int moduleIndex)
        {
            var wordIndex = moduleIndex >> 6;

            switch (wordIndex)
            {
                case 0:
                    return _bits0;
                case 1:
                    return _bits1;
                case 2:
                    return _bits2;
                case 3:
                    return _bits3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(moduleIndex));
            }
        }

        private void SetWord(int moduleIndex, ulong value)
        {
            var wordIndex = moduleIndex >> 6;

            switch (wordIndex)
            {
                case 0:
                    _bits0 = value;
                    break;
                case 1:
                    _bits1 = value;
                    break;
                case 2:
                    _bits2 = value;
                    break;
                case 3:
                    _bits3 = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(moduleIndex));
            }
        }
    }
}
