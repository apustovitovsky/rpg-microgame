using System;

namespace Etheria.Features.StoryletSystem.Authoring
{
    public struct TagSet
    {
        private TagMask _mask;
        private readonly ushort[] _stacks;

        public readonly TagMask Tags => _mask;
        public readonly int Capacity => _stacks?.Length ?? 0;

        public TagSet(int tagCapacity)
        {
            _mask = TagMask.Empty;
            _stacks = new ushort[tagCapacity];
        }

        public readonly bool Contains(TagId tag)
        {
            return GetStack(tag) > 0;
        }

        public readonly ushort GetStack(TagId tag)
        {
            if (_stacks == null)
            {
                return 0;
            }

            return _stacks[tag.Index];
        }

        public readonly bool TryGetStack(TagId tag, out ushort count)
        {
            count = GetStack(tag);
            return count > 0;
        }

        public readonly bool Matches(TagQuery query)
        {
            return query.Matches(_mask);
        }

        public void Add(TagId tag, ushort amount = 1)
        {
            if (amount == 0)
            {
                return;
            }

            ref var stack = ref _stacks[tag.Index];

            if (stack == 0)
            {
                _mask = _mask.With(TagMask.FromId(tag));
            }

            checked
            {
                stack += amount;
            }
        }

        public void Remove(TagId tag, ushort amount = 1)
        {
            if (amount == 0)
            {
                return;
            }

            ref var stack = ref _stacks[tag.Index];

            if (stack == 0)
            {
                return;
            }

            if (amount >= stack)
            {
                stack = 0;
                _mask = _mask.Without(TagMask.FromId(tag));
                return;
            }

            stack -= amount;
        }

        public void Set(TagId tag, ushort value)
        {
            ref var stack = ref _stacks[tag.Index];

            stack = value;

            var tagMask = TagMask.FromId(tag);

            if (value > 0)
            {
                _mask = _mask.With(tagMask);
            }
            else
            {
                _mask = _mask.Without(tagMask);
            }
        }

        public void Clear(TagId tag)
        {
            Set(tag, 0);
        }

        public void ClearAll()
        {
            _mask = TagMask.Empty;

            if (_stacks != null)
            {
                Array.Clear(_stacks, 0, _stacks.Length);
            }
        }
    }
}
