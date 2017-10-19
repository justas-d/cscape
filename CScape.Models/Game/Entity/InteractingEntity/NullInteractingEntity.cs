namespace CScape.Models.Game.Entity.InteractingEntity
{
    public class NullInteractingEntity : IInteractingEntity
    {
        public short Id { get; } = -1;
        public Entity Entity { get; } = null;

        private NullInteractingEntity()
        {
            
        }

        public static NullInteractingEntity Instance { get; } = new NullInteractingEntity();
    }
}