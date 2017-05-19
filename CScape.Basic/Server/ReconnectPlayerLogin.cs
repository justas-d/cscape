using System;
using System.Net.Sockets;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Basic.Server
{
    public class ReconnectPlayerLogin : IPlayerLogin
    {
        public Socket NewConnection { get; }
        public Player Existing { get; }
        public int SignlinkUid { get; }

        public ReconnectPlayerLogin([NotNull] Player existing, [NotNull] Socket newConnection, int signlinkUid)
        {
            NewConnection = newConnection ?? throw new ArgumentNullException(nameof(newConnection));
            Existing = existing ?? throw new ArgumentNullException(nameof(existing));
            SignlinkUid = signlinkUid;
        }

        public void Transfer(IMainLoop ignored)
        {
            if (!Existing.Connection.TryReinitialize(NewConnection, SignlinkUid))
                return; // failed to reinitialize socket

            Existing.Log.Debug(this, $"Reconnected client iid {Existing.UniqueEntityId} Disposed? {Existing.Connection.IsDisposed}");
        }
    }
}