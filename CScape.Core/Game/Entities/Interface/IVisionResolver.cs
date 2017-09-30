namespace CScape.Core.Game.Entities.Interface
{
    public interface IVisionResolver : IEntityComponent
    {
        bool CanBeSeenBy(Entity ent);
    }
}