using System.Collections.Generic;

namespace CScape.Core.Game.Interface
{
    /// <summary>
    /// Provides items to an interface item manager.
    /// </summary>
    public interface IItemProvider : IReadOnlyList<(int id, int amount)>
    {
        int GetId(int idx);
        void SetId(int idx, int value);

        int GetAmount(int idx);
        void SetAmount(int idx, int value);

        new (int id, int amount) this[int index] { get; set; }
    }
}