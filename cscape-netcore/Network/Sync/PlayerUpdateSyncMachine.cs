using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Network.Sync
{
    public sealed class PlayerUpdateSyncMachine : EntityStateSyncMachine<Player>
    {
        [NotNull]
        private Player _local;
        private bool _isLocalNew;

        [NotNull]
        private Blob _updateBlob;

        public void SetLocalPlayer(Player player)
        {
            _local = player;
            _isLocalNew = true;
        }

        public const int Packet = 81;

        public PlayerUpdateSyncMachine(GameServer server) : base(server)
        {
            // todo : figure out the max size of the update flag blob.
            _updateBlob = new Blob(0x400);
        }

        public void PushPlayer(Player player, bool isLocal)
        {
            if (isLocal)
            {
                _local = player;
                _isLocalNew = true;
            }
            else
                AddNew(player);
        }

        public void Clear(Player newLocal)
        {
            ClearEnts();
            SetLocalPlayer(newLocal);
        }

        public override int Order => Constant.SyncMachineOrder.PlayerUpdate;

        public override void Synchronize(OutBlob stream)
        {
            stream.BeginPacket(Packet);

            stream.BeginBitAccess();

            #region local
            /* Types of updates
            * 
            *  0 (just flag the player)
            *      No further reading. Local player is queued for a flag update.
            *      
            *  1 (walk)
            *      3 bits - movement direction
            *      1 bit  - should queue flag update?
            *      
            *  2 (run)
            *      3 bits - movement direction 1
            *      3 bits - movement direction 2
            *      1 bit  - should queue flag update?
            *      
            *  3 (init)
            *      2 bits - z plane
            *      1 bit  - should set flag to true when calling setPos?
            *      1 bit  - should queue flag update?
            *      7 bits - local y
            *      7 bits - local x
            */

            // 3
            if (_isLocalNew)
            {
                stream.WriteBits(1,
                    1); // continue reading? or just "does need updating at all"? If 0, no flag updates will be read for local.

                stream.WriteBits(2, 3); // type

                stream.WriteBits(2, _local.Position.Z); // plane
                // todo : figure out the bit after the z plane in local player updating 
                stream.WriteBits(1, 1); // setPos flag??
                stream.WriteBits(1, _local.HasFlags); // add to needs updating list
                stream.WriteBits(7, _local.Position.LocalY); // local y
                stream.WriteBits(7, _local.Position.LocalX); // local x
            }
            // 1
            else if (_local.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Walk)
            {
                stream.WriteBits(3, _local.Movement.MoveUpdate.Dir1);
                stream.WriteBits(1, _local.HasFlags); // add to needs updating list
            }
            // 2
            else if (_local.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Run)
            {
                stream.WriteBits(3, _local.Movement.MoveUpdate.Dir1);
                stream.WriteBits(3, _local.Movement.MoveUpdate.Dir2);
                stream.WriteBits(1, _local.HasFlags); // add to needs updating list
            }
            // 0
            else
                stream.WriteBits(1, 0); // 0

            #endregion

            WriteExisting(stream, _local);
            WriteNew(stream);
            stream.EndBitAccess();
            WriteFlags(stream);

            stream.EndPacket();
        }

        public override void WriteNew(Blob stream)
        {
            while (NewEnts.Count > 0)
            {
                var player = NewEnts.Dequeue();
                stream.WriteBits(11, SyncEnts.Count); // id

                /*
                 * 1 bit - add to upd list?
                 * todo : 1 bit - setpos flag
                 * 5 bit - y delta from local
                 * 5 bit - x delta from local
                 */

                /*
                 *  Since we're adding a new player to the sync list,
                 *  we need to send initial update flags.
                 *  Those would be the facing direction as well as
                 *  appearance.
                 */
                WriteAppearance(player, _updateBlob);

                stream.WriteBits(1, 1); // has flags. Since this is somebody we haven't seen, write their appearance.
                stream.WriteBits(1, 1); // todo :  setpos flag
                stream.WriteBits(5, _local.Y - player.Y); // ydelta
                stream.WriteBits(5, _local.X - player.X); // xdelta

                SyncEnts = SyncEnts.Add(player);
            }
            stream.WriteBits(11, 2047);
        }

        public override void WriteFlags(Blob stream)
        {
            foreach (var p in SyncEnts)
            {
                if(p.HasFlags == 1)
                    stream.Write(0);
            }

            // if a flag is something that the local player does not need sent
            // to them, just write a byte 0.
        }

        private void WriteAppearance(Player player, Blob stream)
        {
            // todo : somehow make sure we don't write appearance updates twice for the same player
            const int equipSlotSize = 12;

            const int plrObjMagic = 0x100;
            const int itemMagic = 0x200;

            var sizePos = stream.WriteCaret;

            stream.Write((byte)player.Appearance.Gender);
            stream.Write((byte)player.Appearance.Overhead);

            /* 
             * todo : some equipped items conflict with body parts 
             * write body model if chest doesn't conceal the body
             * write head model if head item doesn't fully conceal the head.
             * write beard model if head item doesn't fully conceal the head.
             */
            for (var i = 0; i < equipSlotSize; i++)
            {
                if (player.Appearance[i] != null)
                    stream.Write16((short)(player.Appearance[i].Value + plrObjMagic));
                else
                    stream.Write(0);
            }

            stream.Write(player.Appearance.HairColor);
            stream.Write(player.Appearance.TorsoColor);
            stream.Write(player.Appearance.LegColor);
            stream.Write(player.Appearance.FeetColor);
            stream.Write(player.Appearance.SkinColor);

            // player animation indices
            stream.Write16(0x328); // standAnimIndex
            stream.Write16(0x337); // standTurnAnimIndex
            stream.Write16(0x333); // walkAnimIndex
            stream.Write16(0x334); // turn180AnimIndex
            stream.Write16(0x335); // turn90CWAnimIndex
            stream.Write16(0x336); // turn90CCWAnimIndex
            stream.Write16(0x338); // runAnimIndex

            stream.Write64(StringToLong(player.Username));
            stream.Write(128); //cmb
            stream.Write16(0); // ...skill???

            stream.Buffer[sizePos] = (byte)(stream.WriteCaret - sizePos - 1);

            // todo : proper calculation of cmb lvl
        }

        //smh
        private static long StringToLong(string s)
        {
            var l = 0L;

            foreach (var c in s)
            {
                l *= 37L;
                if (c >= 'A' && c <= 'Z') l += 1 + c - 65;
                else if (c >= 'a' && c <= 'z') l += 1 + c - 97;
                else if (c >= '0' && c <= '9') l += 27 + c - 48;
            }

            while (l % 37L == 0L && l != 0L)
                l /= 37L;

            return l;
        }

    }
}
 