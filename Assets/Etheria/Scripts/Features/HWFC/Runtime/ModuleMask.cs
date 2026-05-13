using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Features.HWFC
{
	[Serializable]
	public struct ModuleSet : ICollection<Module>
	{
		[SerializeField]
		private BitMask256 _mask;

		[SerializeField]
		private float _entropy;

		[SerializeField]
		private bool _entropyOutdated;

		public ModuleSet(bool initializeFull)
		{
			_mask = initializeFull ? UniverseMask : BitMask256.Empty;
			_entropy = 0f;
			_entropyOutdated = true;
		}

		public ModuleSet(ModuleSet source)
		{
			_mask = source._mask;
			_entropy = source.Entropy;
			_entropyOutdated = false;
		}

		public ModuleSet(BitMask256 mask)
		{
			_mask = mask;
			_entropy = 0f;
			_entropyOutdated = true;
		}

		public int Count => _mask.Count;

		public bool Empty => _mask.IsEmpty;

		public bool Full => _mask == UniverseMask;

		public bool IsReadOnly => false;

		public float Entropy
		{
			get
			{
				if (_entropyOutdated)
				{
					_entropy = CalculateEntropy();
					_entropyOutdated = false;
				}

				return _entropy;
			}
		}

		public static ModuleSet EmptySet => new(BitMask256.Empty);

		public static ModuleSet FullSet => new(UniverseMask);

		private static int ModuleCount => 0;

		private static BitMask256 UniverseMask
		{
			get
			{
				var mask = BitMask256.Empty;

				for (int i = 0; i < ModuleCount; i++)
					mask |= BitMask256.FromIndex(i);

				return mask;
			}
		}

		public static ModuleSet FromEnumerable(IEnumerable<Module> source)
		{
			var result = new ModuleSet(BitMask256.Empty);

			foreach (var module in source)
				result.Add(module);

			return result;
		}

		public void Add(Module module)
		{
			var updated = _mask | BitMask256.FromIndex(module.Index);

			if (updated == _mask)
				return;

			_mask = updated;
			_entropyOutdated = true;
		}

		public void Add(ModuleSet set)
		{
			var updated = _mask | set._mask;

			if (updated == _mask)
				return;

			_mask = updated;
			_entropyOutdated = true;
		}

		public bool Remove(Module module)
		{
			var moduleMask = BitMask256.FromIndex(module.Index);

			if (!_mask.Overlaps(moduleMask))
				return false;

			_mask = _mask.Without(moduleMask);
			_entropyOutdated = true;
			return true;
		}

		public void Remove(ModuleSet set)
		{
			var updated = _mask.Without(set._mask);

			if (updated == _mask)
				return;

			_mask = updated;
			_entropyOutdated = true;
		}

		/// <summary>
		/// Removes all modules that are not in the supplied set.
		/// </summary>
		public void Intersect(ModuleSet moduleSet)
		{
			var updated = _mask & moduleSet._mask;

			if (updated == _mask)
				return;

			_mask = updated;
			_entropyOutdated = true;
		}

		public bool Contains(Module module)
		{
			return _mask.ContainsIndex(module.Index);
		}

		public bool Contains(int index)
		{
			return _mask.ContainsIndex(index);
		}

		public void Clear()
		{
			if (_mask.IsEmpty)
				return;

			_mask = BitMask256.Empty;
			_entropyOutdated = true;
		}

		public void CopyTo(Module[] array, int arrayIndex)
		{
			foreach (var module in this)
			{
				array[arrayIndex] = module;
				arrayIndex++;
			}
		}

		public IEnumerator<Module> GetEnumerator()
		{
			for (int i = 0; i < ModuleCount; i++)
			{
				if (_mask.ContainsIndex(i))
					yield return default;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private float CalculateEntropy()
		{
			if (_mask.IsEmpty)
				return float.PositiveInfinity;

			float total = 0f;
			float entropySum = 0f;

			foreach (var module in this)
			{
				float probability = 0;

				total += probability;
				entropySum += module.PLogP;
			}

			if (total <= 0f)
				return float.PositiveInfinity;

			return -entropySum / total + Mathf.Log(total);
		}
	}
}