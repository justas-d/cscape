using System.Linq;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;

namespace CScape.Basic.Model
{
    /// <summary>
    /// Provides items from the player model.
    /// </summary>
    public class ItemProviderModel : PlayerModelLeaf, IItemProvider
    {
        public int Size { get; set; }

        public int[] Ids { get; private set; }
        public int[] Amounts { get; private set; }

        // todo : ItemProviderModel employs a hack in order to let EF Core properly map it's primitive array types that store id and amounts.
        public string DbIds
        {
            get => string.Join(",", Ids);
            set => Ids = value.Split(',').Select(int.Parse).ToArray();
        }

        public string DbAmounts
        {
            get => string.Join(",", Amounts);
            set => Amounts = value.Split(',').Select(int.Parse).ToArray();
        }

        private ItemProviderModel()
        {

        }

        public (int id, int amount) this[int i]
        {
            get
            {
                if (this.IsEmptyAtIndex(i))
                    return (ItemHelper.EmptyId, ItemHelper.EmptyAmount);

                return (Ids[i], Amounts[i]);
            }
            set
            {
                if (this.IsEmptyAtIndex(i))
                    return;

                Ids[i] = value.id;
                Amounts[i] = value.amount;
            }
        }

        public ItemProviderModel(int size)
        {
            Size = size;
            Ids = new int[size];
            Amounts = new int[size];
        }
    }
}