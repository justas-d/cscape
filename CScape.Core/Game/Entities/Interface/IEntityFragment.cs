using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Interface
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