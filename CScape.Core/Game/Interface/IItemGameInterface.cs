using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Interface;

namespace CScape.Core.Game.Interfaces
{
    public interface IItemGameInterface : IGameInterface
    {
        IItemContainer Container { get; }
    }
}