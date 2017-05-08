using System;
using CScape.Game.Entity;
using CScape.Game.Item;
using CScape.Network;

namespace CScape.Dev.Providers
{
    public class ServerDatabase : IDatabase, IDisposable
    {
        public IPacketLengthLookup Packet { get; }
        public IPlayerDatabase Player => _playerDb;
        public IItemDatabase Item { get; }

        private PlayerDb _playerDb; 

        public ServerDatabase(string packetJsonDir)
        {
            Packet = new PacketLookup(packetJsonDir);
            _playerDb = new PlayerDb();
            _playerDb.Database.EnsureCreated();
            Item = new ItemDatabase();
        }

        public void Dispose()
        {
            _playerDb?.Dispose();
            _playerDb = null;
        }
    }

    public class BasicItem : IItemDefinition
    {
        public bool Equals(IItemDefinition other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ItemId == other.ItemId;
        }

        public int ItemId { get; }
        public string Name { get; }
        public int MaxAmount { get; }
        public bool IsTradable { get; }
        public float Weight { get; }
        public bool IsNoted { get; }
        public int NoteSwitchId { get; }

        public BasicItem(int itemId, string name, int maxAmount, bool isTradable, float weight, bool isNoted, int noteSwitchId)
        {
            ItemId = itemId;
            Name = name;
            MaxAmount = maxAmount;
            IsTradable = isTradable;
            Weight = weight;
            IsNoted = isNoted;
            NoteSwitchId = noteSwitchId;
        }

        public void UseWith(Player user, IItemDefinition other)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class ItemDatabase : IItemDatabase
    {
        public IItemDefinition Get(int id)
        {
            return new BasicItem(id, "Dummy", int.MaxValue, true, 1, false, -1);
        }
    }
}