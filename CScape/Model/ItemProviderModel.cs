using CScape.Game.Interface;
using CScape.Game.Item;

namespace CScape.Model
{
    /// <summary>
    /// Provides items from the player model.
    /// </summary>
    public class ItemProviderModel : IItemProvider, IPlayerForeignModel
    {
        public string ForeignKey { get; set; }
        PlayerModel IForeignModelObject<string, PlayerModel>.Model { get; set; }

        public int Size { get; set; }
        public int[] Ids { get; set; }
        public int[] Amounts { get; set; }

        public int[] Ids { get; set; }
        public int[] Amounts { get; set; }

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

        private ItemProviderModel()
        {

        }

        public ItemProviderModel(int size)
        {
            Size = size;
            Ids = new int[size];
            Amounts = new int[size];
        }
    }
}