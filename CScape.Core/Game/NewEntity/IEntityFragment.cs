using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public interface IEntityFragment
    {
        Entity Parent { get; }

        /// <summary>
        /// The lower the priority, the sooner it'll be updated.
        /// </summary>
        int Priority { get; }

        void ReceiveMessage([NotNull] EntityMessage msg);
    }
}