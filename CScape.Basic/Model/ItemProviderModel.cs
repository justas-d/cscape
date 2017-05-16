using System.Linq;

namespace CScape.Basic.Model
{
    public sealed class ItemProviderModel : PlayerModelLeaf
    {
        public int Size { get; set; }

        public int[] Ids { get; private set; }
        public int[] Amounts { get; private set; }

        public string DbIds
        {
            get => string.Join<int>(";", Ids);
            set => Ids = value.Split(';').Select(int.Parse).ToArray();
        }

        public string DbAmounts
        {
            get => string.Join<int>(";", Amounts);
            set => Amounts = value.Split(';').Select(int.Parse).ToArray();
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