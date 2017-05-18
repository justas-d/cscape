namespace CScape.Core.Game.Entity
{
    /// <summary>
    /// Defines a transform that will be synced to the client.
    /// </summary>
    public interface IClientTransform
    {
        /// <summary>
        /// Returns the base coordinates of the current <see cref="ClientRegion"/>
        /// </summary>
        (int x, int y) Base { get; }

        /// <summary>
        /// Returns the client region coordinates of the current region.
        /// </summary>
        (int x, int y) ClientRegion { get; }

        /// <summary>
        /// Returns the local coordinates of the transform in the current <see cref="ClientRegion"/>
        /// </summary>
        (int x, int y) Local { get; }
    }
}