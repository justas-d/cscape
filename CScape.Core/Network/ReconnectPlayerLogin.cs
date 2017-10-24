using System;
using System.Net.Sockets;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity;
using CScape.Models;
using CScape.Models.Extensions;
using JetBrains.Annotations;

namespace CScape.Core.Network
{
    public class ReconnectPlayerLogin : IPlayerLogin
    {
        public Socket NewConnection { get; }
        public int SignlinkUid { get; }
        public EntityHandle Existing { get; }

        private ILogger Log => Existing.System.Server.Services.ThrowOrGet<ILogger>();

        public ReconnectPlayerLogin([NotNull] EntityHandle existing, [NotNull] Socket newConnection, int signlinkUid)
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

            var net = entity.GetNetwork();
            if (net == null)
                return;

            if (!net.TryReinitializeUsing(NewConnection, SignlinkUid))
            {
                Log.Normal(this, $"Attempted but failed to reconnected entity {Existing} Disposed?");
                return;
            }

            Log.Normal(this, $"Reconnected entity {Existing}.");
        }
    }
}