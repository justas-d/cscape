using CScape.Core.Game.Entity;

namespace CScape.Core.Game.Interface
{
    /// <summary>
    /// Defines a button handler for an interface.
    /// </summary>
    public interface IButtonHandler
    {
        void OnButtonPressed(Player player, int id);
    }
}