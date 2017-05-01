using System;
using CScape.Data;
using CScape.Game.World;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    // todo : npc
    public class Npc : WorldEntity
    {
        [Flags]
        public enum UpdateFlags
        {
            
        }

        public UpdateFlags Flags { get; private set; }

        public void SetFlag(UpdateFlags flag)
            => Flags |= flag;

        [NotNull] public MovementController Movement { get; }

        public Npc(
            [NotNull] GameServer server, 
            [NotNull] IdPool idPool,
            ushort x, ushort y, byte z) : base(server, idPool)
        {
            //Movement = new MovementController(this);
        }

        public override void SyncTo(ObservableSyncMachine sync, Blob blob, bool isNew)
        {
            throw new NotImplementedException();
            //if(isNew)
            //sync.PushToNpcSyncMachine(this);
        }

        public override bool CanSee(IWorldEntity ent)
        {
            throw new NotImplementedException();
        }

        public override void Update(MainLoop loop)
        {
            throw new NotImplementedException();
        }
    }
}