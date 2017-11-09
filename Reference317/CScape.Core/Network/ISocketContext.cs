using System.Net.Sockets;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network
{
    public interface ISocketContext
    {
        long DeadForMs { get; }
        CircularBlob InStream { get; }
        bool IsDisposed { get; }
        OutBlob OutStream { get; }
        int SignlinkId { get; }

        bool CanReinitialize(int signlinkId);
        void Dispose();
        bool FlushOutputStream();
        bool IsConnected();
        bool TryReinitialize([NotNull] Socket socket, int signlinkId);
        bool Update(long deltaTime);
    }
}