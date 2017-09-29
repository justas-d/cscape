using System;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    /// <summary>
    /// Responsible for keeping the player in sync.
    /// </summary>
    public sealed class NetPlayerSyncComponent : IEntityComponent
    {
        [NotNull]
        public ISocketContext Socket { get; }
        public Entity Parent { get; }

        public NetPlayerSyncComponent(
            [NotNull] Entity parent, [NotNull] ISocketContext socket)
        {
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public void Update(IMainLoop loop)
        {

        }

        public void ReceiveMessage(EntityMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}