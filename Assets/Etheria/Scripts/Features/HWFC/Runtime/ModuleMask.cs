using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Features.HWFC
{
    [Serializable]
    public struct ModuleSet : IEnumerable<int>, IEquatable<ModuleSet>
    {
        [SerializeField]
        private BitMask256 _mask;

        [SerializeField]
        private int _moduleCount;

        public ModuleSet(int moduleCount, bool filled)
        {
            ValidateModuleCount(moduleCount);

            _moduleCount = moduleCount;
            _mask = filled ? CreateUniverseMask(moduleCount) : BitMask256.Empty;
        }

        private ModuleSet(int moduleCount, BitMask256 mask)
        {
            ValidateModuleCount(moduleCount);

            _moduleCount = moduleCount;
            _mask = mask;
        }

        public int ModuleCount => _moduleCount;

        public int Count => _mask.Count;

        public bool IsEmpty => _mask.IsEmpty;

        public bool IsFull => _mask == CreateUniverseMask(_moduleCount);

        public static ModuleSet Empty(int moduleCount)
        {
            return new ModuleSet(moduleCount, filled: false);
        }

        public static ModuleSet Full(int moduleCount)
        {
            return new ModuleSet(moduleCount, filled: true);
        }

        public static ModuleSet Single(int moduleCount, int moduleIndex)
        {
            var set = Empty(moduleCount);
            set.Add(moduleIndex);
            return set;
        }

        public bool Contains(int moduleIndex)
        {
            ValidateModuleIndex(moduleIndex);
            return _mask.ContainsIndex(moduleIndex);
        }

        public void Add(int moduleIndex)
        {
            ValidateModuleIndex(moduleIndex);
            _mask = _mask | BitMask256.FromIndex(moduleIndex);
        }

        public bool Remove(int moduleIndex)
        {
            ValidateModuleIndex(moduleIndex);
            var itemMask = BitMask256.FromIndex(moduleIndex);
            if (!_mask.Overlaps(itemMask))
            {
                return false;
            }

            _mask = _mask.Without(itemMask);
            return true;
        }

        public void Clear()
        {
            _mask = BitMask256.Empty;
        }

        public bool Overlaps(ModuleSet other)
        {
            EnsureCompatible(other);
            return _mask.Overlaps(other._mask);
        }

        public bool ContainsAll(ModuleSet other)
        {
            EnsureCompatible(other);
            return _mask.ContainsAll(other._mask);
        }

        public void IntersectWith(ModuleSet other)
        {
            EnsureCompatible(other);
            _mask = _mask & other._mask;
        }

        public void UnionWith(ModuleSet other)
        {
            EnsureCompatible(other);
            _mask = _mask | other._mask;
        }

        public void ExceptWith(ModuleSet other)
        {
            EnsureCompatible(other);
            _mask = _mask.Without(other._mask);
        }

        public float CalculateEntropy(ModuleModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (_moduleCount != model.ModuleCount)
            {
                throw new ArgumentException("ModuleSet and ModuleModel must use the same module count.", nameof(model));
            }

            if (_mask.IsEmpty)
            {
                return float.PositiveInfinity;
            }

            double totalWeight = 0d;
            double totalWeightLogWeight = 0d;

            foreach (var moduleIndex in this)
            {
                totalWeight += model.Weights[moduleIndex];
                totalWeightLogWeight += model.WeightLogWeights[moduleIndex];
            }

            if (totalWeight <= 0d)
            {
                return float.PositiveInfinity;
            }

            return (float)(Math.Log(totalWeight) - (totalWeightLogWeight / totalWeight));
        }

        public IEnumerator<int> GetEnumerator()
        {
            for (var moduleIndex = 0; moduleIndex < _moduleCount; moduleIndex++)
            {
                if (_mask.ContainsIndex(moduleIndex))
                {
                    yield return moduleIndex;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(ModuleSet other)
        {
            return _moduleCount == other._moduleCount && _mask.Equals(other._mask);
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleSet other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_mask.GetHashCode() * 397) ^ _moduleCount;
            }
        }

        public static bool operator ==(ModuleSet left, ModuleSet right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModuleSet left, ModuleSet right)
        {
            return !left.Equals(right);
        }

        private static BitMask256 CreateUniverseMask(int moduleCount)
        {
            var mask = BitMask256.Empty;

            for (var moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
            {
                mask |= BitMask256.FromIndex(moduleIndex);
            }

            return mask;
        }

        private static void ValidateModuleCount(int moduleCount)
        {
            if (moduleCount < 0 || moduleCount > BitMask256.Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(moduleCount));
            }
        }

        private void ValidateModuleIndex(int moduleIndex)
        {
            if (moduleIndex < 0 || moduleIndex >= _moduleCount)
            {
                throw new ArgumentOutOfRangeException(nameof(moduleIndex));
            }
        }

        private void EnsureCompatible(ModuleSet other)
        {
            if (_moduleCount != other._moduleCount)
            {
                throw new InvalidOperationException("Cannot combine module sets with different module counts.");
            }
        }
    }
}
