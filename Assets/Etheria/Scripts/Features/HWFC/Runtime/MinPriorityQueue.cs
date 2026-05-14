using System.Collections.Generic;

namespace Etheria.Features.HWFC
{
    public sealed class MinPriorityQueue<T>
    {
        private readonly List<(T Item, float Priority)> _heap = new List<(T Item, float Priority)>();

        public int Count => _heap.Count;

        public void Enqueue(T item, float priority)
        {
            _heap.Add((item, priority));
            SiftUp(_heap.Count - 1);
        }

        public bool TryDequeue(out T item, out float priority)
        {
            if (_heap.Count == 0)
            {
                item = default;
                priority = default;
                return false;
            }

            (item, priority) = _heap[0];

            int last = _heap.Count - 1;
            _heap[0] = _heap[last];
            _heap.RemoveAt(last);

            if (_heap.Count > 0)
                SiftDown(0);

            return true;
        }

        public void Clear() => _heap.Clear();

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;

                if (_heap[parent].Priority <= _heap[index].Priority)
                    break;

                (_heap[parent], _heap[index]) = (_heap[index], _heap[parent]);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            while (true)
            {
                int left = index * 2 + 1;
                int right = left + 1;
                int smallest = index;

                if (left < _heap.Count && _heap[left].Priority < _heap[smallest].Priority)
                    smallest = left;

                if (right < _heap.Count && _heap[right].Priority < _heap[smallest].Priority)
                    smallest = right;

                if (smallest == index)
                    break;

                (_heap[index], _heap[smallest]) = (_heap[smallest], _heap[index]);
                index = smallest;
            }
        }
    }
}
