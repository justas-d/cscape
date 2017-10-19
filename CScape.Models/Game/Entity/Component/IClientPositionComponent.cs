namespace CScape.Models.Game.Entity.Component
{
    public interface IClientPositionComponent : IEntityComponent
    {
        IPosition Base { get; }
        IPosition ClientRegion { get; }
        IPosition Local { get; }
    }
}