using CScape.Core.Data;

namespace CScape.Core.Game.Entity
{
    public static class EntityHelper
    {
        public const int InteractingEntityPlayerFlag = 0x8000;

        public static void TryResetInteractingEntity(IMovingEntity ent)
        {
            // reset InteractingEntity if we can't see it anymore.
            if (ent.InteractingEntity != null && !ent.CanSee(ent.InteractingEntity))
                ent.InteractingEntity = null;
        }

        public static void WriteInteractingEntityFlag(IMovingEntity ent, 
            int categoryId, Blob stream)
        {
            if (ent.InteractingEntity == null)
                stream.Write16(-1);
            else
            {
                if (ent.InteractingEntity is Player interactPlayer)
                    stream.Write16((short) (interactPlayer.Pid | InteractingEntityPlayerFlag));
                else
                    stream.Write16((short)categoryId);
            }
        }
    }
}
