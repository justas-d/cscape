using System;
using CScape;
using CScape.Network.Packet;
using Microsoft.EntityFrameworkCore;

namespace cscape_dev
{
    public class ServerDatabase : IDatabase, IDisposable
    {
        public IPacketLengthLookup Packet { get; }
        public IPlayerDatabase Player => _playerDb;

        private PlayerDb _playerDb;

        public ServerDatabase(string packetJsonDir)
        {
            Packet = new PacketLookup(packetJsonDir);
            _playerDb = new PlayerDb();
            _playerDb.Database.Migrate();
        }

        public void Dispose()
        {
            _playerDb?.Dispose();
            _playerDb = null;
        }
    }
}