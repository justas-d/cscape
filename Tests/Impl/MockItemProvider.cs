using System.Collections;
using System.Collections.Generic;

namespace CScape.Dev.Tests.Impl
{
    public class MockItemProvider : IItemProvider
    {
        public int Count { get; }

        private readonly int[] _ids;
        private readonly int[] _amnts;

        public MockItemProvider(int size)
        {
            _ids = new int[size];
            _amnts = new int[size];
            Count = size;
        }

        public int GetId(int idx) => _ids[idx];
        public void SetId(int idx, int value) => _ids[idx] = value;
        public int GetAmount(int idx) => _amnts[idx];
        public void SetAmount(int idx, int value) => _amnts[idx] = value;

        public IEnumerator<(int id, int amount)> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return (_ids[i], _amnts[i]);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        (int id, int amount) IItemProvider.this[int i]
        {
            get => (_ids[i], _amnts[i]);
            set
            {
                _ids[i] = value.id;
                _amnts[i] = value.amount;
            }
        }

        (int id, int amount) IReadOnlyList<(int id, int amount)>.this[int i] => (_ids[i], _amnts[i]);
    }
}
