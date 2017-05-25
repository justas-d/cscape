using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network.Packet;

namespace CScape.Core.Network.Sync
{
    public sealed class GroundItemSyncMachine : ISyncMachine
    {
        private readonly Player _local;
        public int Order => SyncMachineConstants.GroundItemSync;
        public bool RemoveAfterInitialize => false;

        // keeps track of what items have we sent initial update packets for
        private readonly HashSet<uint> _tracked = new HashSet<uint>();

        // holds packets sorted by region
        private readonly Dictionary<(int x, int y), List<BaseGroundObjectPacket>> _buckets 
            = new Dictionary<(int x, int y), List<BaseGroundObjectPacket>>();

        private bool _needsSync = false;

        private readonly ILogger _log;

        public GroundItemSyncMachine(IServiceProvider services, Player local)
        {
            _log = services.ThrowOrGet<ILogger>();
            _local = local;
        }

        public void UpdateItem(GroundItem item)
        {
            ((int x, int y) regionGrid, (int x, int y) offset) GetCoords()
            {
                // get item local coords from the perspective of the player's client transform
                var itemLocal =
                    (item.Transform.X - _local.ClientTransform.Base.x, 
                    item.Transform.Y - _local.ClientTransform.Base.y);

                // calc the offset of the item in the 8x8 region it belongs to 
                var offset = (
                    itemLocal.Item1 % 8, 
                    itemLocal.Item2 % 8);

                // calc item's region locals
                var region = (
                    itemLocal.Item1 - offset.Item1, 
                    itemLocal.Item2 - offset.Item2);

                // evaluate offsets by finding the remainder left by the local snap to region grid
                return (region, offset);
            }

            void SendInitial()
            {
                var coords = GetCoords();

                var p = new SpawnGroundItemPacket(item, coords.offset);

                // place the packet into the appropriate region bucket
                AddPacket(p, coords.regionGrid);
    
                // log the sync
                _local.DebugMsg(
                    $"(ITEM) initial item: region: ({coords.regionGrid.Item1} {coords.regionGrid.Item2}) offset: ({coords.offset.Item1} {coords.offset.Item2}) invalid: {p.IsInvalid}",
                    ref _local.DebugEntitySync);
            }

            void AddPacket(BaseGroundObjectPacket packet, (int x,int y) regionGrid)
            {
                // lazy init the bucket
                if (!_buckets.ContainsKey(regionGrid))
                    _buckets.Add(regionGrid, new List<BaseGroundObjectPacket>());

                _buckets[regionGrid].Add(packet);

                // set sync flag
                _needsSync = true;

                // log invalid offsets
                if (packet.IsInvalid)
                    _log.Warning(this, $"Invalid spawn ground item packet at offsets: ({regionGrid.x} {regionGrid.y})");
            }


            // figure out if the item needs to be synced
            if (_tracked.Contains(item.UniqueEntityId))
            {
                // figure out what update the item needs

                // amount update flag
                if (item.NeedsAmountUpdate)
                {
                    var coords = GetCoords();
                    AddPacket(new UpdateGroundItemAmountPacket(item, coords.offset), coords.regionGrid);
                }
                // remove flag
                if (item.IsDestroyed)
                {
                    var coords = GetCoords();
                    AddPacket(new DeleteGroundItemPacket(item, coords.offset), coords.regionGrid);
                    _tracked.Remove(item.UniqueEntityId);
                }
            }
            else
            {
                // check whether our local is the one who dropped it..
                // ..or the item is public
                if(_local.CanSeeItem(item))
                {
                    // we should start tracking the item now
                    _tracked.Add(item.UniqueEntityId);
                    SendInitial();
                }
            }
        }

        public void Clear()
        {
            _tracked.Clear();
        }

        public void Synchronize(OutBlob stream)
        {
            // don't sync empty buckets
            if (!_needsSync)
                return;

            _needsSync = false;

            foreach (var kvp in _buckets)
            {
                // dont sync empty bucket
                if (!kvp.Value.Any())
                    continue;

                // sync bucket
                var wrap = new EmbeddedRegionGroundObjectWrapperPacket(kvp.Key, kvp.Value);
                wrap.Send(stream);

                kvp.Value.Clear();
            }
        }

        public void OnReinitialize() { }
    }
}