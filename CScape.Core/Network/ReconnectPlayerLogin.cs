using System;
using System.Net.Sockets;
using CScape.Core;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network;
using CScape.Core.Network.Entity.Component;
using JetBrains.Annotations;

namespace CScape.Basic.Server
{
    public class ReconnectPlayerLogin : IPlayerLogin
    {
        public Socket NewConnection { get; }
        public int SignlinkUid { get; }
        public EntityHandle Existing { get; }

        private ILogger Log => Existing.System.Server.Services.ThrowOrGet<ILogger>();

        public ReconnectPlayerLogin([NotNull] Player existing, [NotNull] Socket newConnection, int signlinkUid)
        {
            NewConnection = newConnection ?? throw new ArgumentNullException(nameof(newConnection));
            Existing = existing ?? throw new ArgumentNullException(nameof(existing));
            SignlinkUid = signlinkUid;
        }

        public void Transfer(IMainLoop ignored)
        {
            if (Existing.IsDead())
                return;

            var entity = Existing.Get();

            var net = entity.Components.Get<NetworkingComponent>();
            if (net == null)
                return;

            if (!net.TryReinitializeUsing(NewConnection, SignlinkUid))
            {
                Log.Normal(this, $"Attempted but failed to reconnected entity {Existing} Disposed?");
                return;
            }

            Log.Normal(this, $"Reconnected client iid {Existing.UniqueEntityId} Disposed? {Existing.Connection.IsDisposed}");
        }
    }
}