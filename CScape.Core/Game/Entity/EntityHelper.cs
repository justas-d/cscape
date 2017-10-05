using CScape.Core.Data;
using CScape.Core.Game.Entities.Message;

namespace CScape.Core.Game.Entity
{
    public static class EntityHelper
    {
        /*
        public const int InteractingEntityPlayerFlag = 0x8000;

        public static void TryResetInteractingEntity(IMovingEntity ent)
        {
            // reset InteractingEntity if we can't see it anymore.
            if (ent.InteractingEntity != null && !ent.CanSee(ent.InteractingEntity))
                ent.InteractingEntity = null;
        }
        */

        public static void WriteHitData(Blob stream, IDamageable ent, bool secondary)
        {
            var data = secondary ? ent.SecondaryHit : ent.PrimaryHit;
            if(data == null)
                data = HitData.Zero;

            stream.Write(data.Damage);
            stream.Write((byte)data.Type);
            stream.Write(data.CurrentHealth);
            stream.Write(data.MaxHealth);
        }

        public static void WriteInteractingEntityFlag(IMovingEntity ent, 
            int categoryId, Blob stream)
        {
            const short reset = -1;

            if (ent.InteractingEntity == null)
                stream.Write16(reset);
            else
            {
                if (ent.InteractingEntity is Player interactPlayer)
                    stream.Write16((short) (interactPlayer.Pid | InteractingEntityPlayerFlag));
                else
                    stream.Write16((short)categoryId);
            }
        }

        public static void WriteFacingDirection(IMovingEntity ent, (ushort x, ushort y)? nullableFacingCoordinate, Blob stream)
        {
            // TODO : replace this null check with a negative check
            if (nullableFacingCoordinate != null)
            {
                var facing = nullableFacingCoordinate.Value;

                stream.Write16((short)((facing.x * 2) + 1));
                stream.Write16((short)((facing.y * 2) + 1));
            }
            else
            {
                stream.Write16((short)
                    (((ent.Movement.LastMovedDirection.x + ent.Transform.X) * 2) + 1));
                stream.Write16((short)
                    (((ent.Movement.LastMovedDirection.y + ent.Transform.Y) * 2) + 1));
            }
        }
    }
}
