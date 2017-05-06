using CScape.Data;
using CScape.Game.Interface;

namespace CScape.Game.Model
{
    /// <summary>
    /// Provides items from the player model.
    /// </summary>
    public class ModelItemProvider : IItemProvider, IPlayerForeignModel
    {
        public int Id { get; private set; }
        string IForeignModelObject<string, PlayerModel>.ForeignKey { get; set; }
        PlayerModel IForeignModelObject<string, PlayerModel>.Model { get; set; }

        public (int id, int amount)[] Items { get; }

        private ModelItemProvider()
        {

        }

        public ModelItemProvider(int size)
        {
            Items = new(int, int)[size];
        }
    }
}