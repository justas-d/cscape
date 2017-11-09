using JetBrains.Annotations;

namespace CScape.Models.Game.Entity
{
    public interface IEntityComponent
    {
        [NotNull]
        IEntity Parent { get; }
        int Priority { get; }

        void ReceiveMessage([NotNull] IGameMessage msg);
    }
}