namespace CScape.Models.Game.Entity.Component
{
    public interface IVisionResolver : IEntityComponent
    {
        /// <summary>
        /// Determines whether this component's parent entity can be seen by the given entity <see cref="ent"/>.
        /// <param name="inRange">Whether this component's entity is in the view range of <see cref="ent"/>.</param>
        /// </summary>
        bool CanBeSeenBy(IEntity ent, bool inRange);
    }
}