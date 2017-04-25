using System;
using CScape.Game.Worldspace;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public class Npc : AbstractEntity, IFlagSyncableEntity
    {
        [Flags]
        public enum UpdateFlags
        {
            
        }

        public UpdateFlags Flags { get; private set; }

        public int HasFlags => Flags != 0 ? 1 : 0;

        public void SetFlag(UpdateFlags flag)
            => Flags |= flag;

        public Npc(
            [NotNull] GameServer server, 
            [NotNull] IdPool idPool,
            ushort x, ushort y, byte z,
            PlaneOfExistance poe = null,
            MovementController movement = null) : base(server, idPool, x,y,z, poe, movement)
        {
        }

        public override void SyncObservable(ObservableSyncMachine sync, Blob blob, bool isNew)
        {
            if(isNew)
                sync.PushToNpcSyncMachine(this);
        }
    }
}