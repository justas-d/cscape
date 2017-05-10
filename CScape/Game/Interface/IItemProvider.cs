using System.Collections.Generic;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Provides items to an interface item manager.
    /// </summary>
    public interface IItemProvider
    {
        int Size { get; }
        int[] Ids { get; }
        int[] Amounts { get; }
        (int id, int amount) this[int i] { get; set; }
    }
}