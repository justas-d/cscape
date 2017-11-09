namespace CScape.Models.Game.Entity.Component
{
    /// <summary>
    /// Defines a way to look up client position data of an entity.
    /// </summary>
    public interface IClientPositionComponent : IEntityComponent
    {
        /// <summary>
        /// The region base coordinate.
        /// </summary>
        IPosition Base { get; }

        /// <summary>
        /// The client region coordinates.
        /// </summary>
        IPosition ClientRegion { get; }
        
        /// <summary>
        /// The in-region local coordinates.
        /// </summary>
        IPosition Local { get; }
    }
}