using System;
using System.Collections;
using System.Collections.Generic;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;
using JetBrains.Annotations;

namespace CScape.Basic.Model
{
    public class ItemProviderSegment : IItemProvider
    {
        [NotNull] private readonly ItemProviderModel _provider;
        private readonly int _offset;

        public int Count { get; }

        public (int id, int amount) this[int index]
        {
            get
            {
                // check if in range
                if (0 > index || index >= Count)
                    return ItemHelper.EmptyItem;

                // get
                return (GetId(index), GetAmount(index));
            }
            set
            {
                // check if in range
                if (0 > index || index >= Count)
                    return;

                SetId(index, value.id);
                SetAmount(index, value.amount);
            }
        }

        public ItemProviderSegment([NotNull] ItemProviderModel provider, int offset, int count)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _offset = offset;

            Count = count - offset;
        }


        public IEnumerator<(int id, int amount)> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int GetId(int idx) => _provider.Ids[_offset + idx];
        public int GetAmount(int idx) => _provider.Amounts[_offset + idx];
        public void SetId(int idx, int value) => _provider.Ids[_offset + idx] = value;
        public void SetAmount(int idx, int value) => _provider.Amounts[_offset + idx] = value;
    }
}