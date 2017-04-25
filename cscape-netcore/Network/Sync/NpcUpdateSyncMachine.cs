using CScape.Game.Entity;

namespace CScape.Network.Sync
{
    public sealed class NpcUpdateSyncMachine : EntityStateSyncMachine<Npc>
    {
        public NpcUpdateSyncMachine(GameServer server) : base(server)
        {
        }

        public override int Order => Constant.SyncMachineOrder.NpcUpdate;

        public override void Synchronize(OutBlob stream)
        {
            // todo : NpcUpdateSyncMachine
        }

        // todo : WriteNewPlayers
        // no range checking needed

        // before writing a new player here, check if it already exists
        // in the _syncPlayers container.

        //   stream.WriteBits(11, 2047); // index (in this case 2047 is the break flag)

        /*
         *  Instead of writing some kind of unique index for the new player,
         *  write the index at which this new player will be stored in
         *  the _syncPlayers container.
         */
        public override void WriteNew(Blob stream)
        {
            throw new System.NotImplementedException();
        }

        public override void WriteFlags(Blob stream)
        {
            throw new System.NotImplementedException();
        }

        public void PushNpc(Npc npc)
            => AddNew(npc);

        public void Clear()
            => ClearEnts();
    }
}