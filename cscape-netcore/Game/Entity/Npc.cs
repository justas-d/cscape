using System;
using CScape.Data;
using CScape.Game.World;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public class Npc : AbstractEntity
    {
        [Flags]
        public enum UpdateFlags
        {
            
        }

        public UpdateFlags Flags { get; private set; }

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
            throw new NotImplementedException();
            //if(isNew)
                //sync.PushToNpcSyncMachine(this);
        }
    }
}