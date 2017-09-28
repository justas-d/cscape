using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public interface IEntityComponent
    {
        Entity Parent { get; }

        void Update([NotNull]IMainLoop loop);
        void ReceiveMessage([NotNull] EntityMessage msg);
    }
}