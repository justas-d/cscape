using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public static class DirectionsProviderExtensions
    {
        public static DirectionDelta GetNextDir(this IDirectionsProvider prov, [NotNull]NewEntity.Entity ent)
        {
            var t = ent.GetTransform();
            return prov.GetNextDir(ent, t.X, t.Y, t.Z);
        }

        public static DirectionDelta GetNextDirOffset(this IDirectionsProvider prov, [NotNull]NewEntity.Entity ent,
            int offX, int offY, int offZ)
        {
            var t = ent.GetTransform();
            return prov.GetNextDir(ent, t.X + offX, t.Y + offY, t.Z + offZ);
        }

        public static bool IsDoneOffset(this IDirectionsProvider prov, [NotNull]NewEntity.Entity ent,
            int offX, int offY, int offZ)
        {
            var t = ent.GetTransform();
            return prov.IsDone(ent, t.X + offX, t.Y + offY, t.Z + offZ);
        }

        public static bool IsDone(this IDirectionsProvider prov, [NotNull]NewEntity.Entity ent)
        {
            var t = ent.GetTransform();
            return prov.IsDone(ent, t.X, t.Y, t.Z);
        }
    }

    public interface IDirectionsProvider
    {
        DirectionDelta GetNextDir([NotNull] NewEntity.Entity ent, int x, int y, int z);
        bool IsDone([NotNull] NewEntity.Entity ent, int x, int y, int z);
    }
}