using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Network.Sync
{
    public abstract class EntityStateSyncMachine<T> : SyncMachine where T : AbstractEntity, IFlagSyncableEntity
    {
        [NotNull] protected ImmutableList<T> SyncEnts { get; set; } = ImmutableList<T>.Empty;
        [NotNull] protected Queue<T> NewEnts { get; } = new Queue<T>();

        protected EntityStateSyncMachine(GameServer server) : base(server)
        {
        }

        public void AddNew(T ent)
        {
            Debug.Assert(!SyncEnts.Contains(ent));
            Debug.Assert(!NewEnts.Contains(ent));
            NewEnts.Enqueue(ent);
        }

        protected void ClearEnts()
        {
            SyncEnts = ImmutableList<T>.Empty;
            NewEnts.Clear();
        }

        public void WriteExisting(Blob stream, IObserver observer)
        {
            var countPos = stream.BitWriteCaret;
            stream.WriteBits(8, 0); // placeholder for the count of existing update ents

            var written = 0;
            foreach (var ent in SyncEnts)
            {
                // check if the entity is still qualified for updates
                if (ent.IsDestroyed || !observer.CanSee(ent))
                {
                    // send remove payload
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 3); // type
                    SyncEnts = SyncEnts.Remove(ent);
                    continue;
                }

                // run
                if (ent.Movement?.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Run)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 2); // type
                    stream.WriteBits(3, ent.Movement.MoveUpdate.Dir1);
                    stream.WriteBits(3, ent.Movement.MoveUpdate.Dir2);
                    stream.WriteBits(1, ent.HasFlags); // needs update?
                }
                // walk
                else if (ent.Movement?.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Walk)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 1); // type
                    stream.WriteBits(3, ent.Movement.MoveUpdate.Dir1);
                    stream.WriteBits(1, ent.HasFlags); // needs update?
                }
                // no pos update, just needs a flag update
                else if (ent.HasFlags == 1)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 0); // type
                }
                // absolutely no update
                else
                {
                    stream.WriteBits(1, 0); // is not noop?
                }

                written++;
            }

            var actualPos = stream.BitWriteCaret;
            stream.BitWriteCaret = countPos;

            // write the placeholder
            stream.WriteBits(8, written); 

            stream.BitWriteCaret = actualPos;
        }

        public abstract void WriteNew(Blob stream);

        public abstract void WriteFlags(Blob stream);
    }
}