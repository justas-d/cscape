namespace CScape.Core.Game.Entities
{
    public interface IVisionResolver : IEntityComponent
    {
        bool CanBeSeenBy(Entity ent);
    }
}