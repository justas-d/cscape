using System;
using CScape.Core.Data;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
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

        public Npc(IServiceProvider service):  base(service)
        {
        }

        public override void Update(IMainLoop loop)
        {
            throw new NotImplementedException();
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
    }
}