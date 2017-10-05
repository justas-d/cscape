namespace CScape.Core.Game.Entities.InteractingEntity
{
    public class NullInteractingEntity : IInteractingEntity
    {
        public int Id { get; } = -1;
        public Entity Entity { get; } = null;

        private NullInteractingEntity()
        {
            
        }

        public static NullInteractingEntity Instance { get; } = new NullInteractingEntity();
    }
}